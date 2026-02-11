using fireMCG.PathOfLayouts.IO;
using fireMCG.PathOfLayouts.Manifest;
using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.Srs;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Core
{
    public sealed class Bootstrap : MonoBehaviour
    {
        public static Bootstrap Instance { get; private set; }
        public CampaignManifestService ManifestService { get; private set; }
        public SrsService SrsService { get; private set; }
        public bool IsReady { get; private set; } = false;

        private CancellationTokenSource _tokenSource;

        private void Awake()
        {
            if(Instance is not null)
            {
                Destroy(gameObject);

                return;
            }

            Instance = this;

            _tokenSource = new CancellationTokenSource();
        }

        private async void Start()
        {
            try
            {
                await InitializeAsync(_tokenSource.Token);

                IsReady = true;
                MessageBusManager.Resolve.Publish(new OnBootstrapReadyMessage());
            }
            catch (System.OperationCanceledException) { }
            catch (System.Exception e)
            {
                Debug.LogError(e);

                Application.Quit();
            }
        }

        private void OnDestroy()
        {
            if(Instance == this)
            {
                Instance = null;
            }

            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }

        private async Task InitializeAsync(CancellationToken token)
        {
            ManifestService = new CampaignManifestService();
            await ManifestService.LoadManifestAsync(token);

            SrsService = new SrsService();

            try
            {
                await SrsService.LoadSrsSaveDataAsync(token);
                MessageBusManager.Resolve.Publish(new RegisterPersistableMessage(SrsService));
            }
            catch (System.OperationCanceledException)
            {
                throw;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Bootstrap.InitializeAsync error, Srs failed to load, continuing with defaults. e={e}");

                SrsService.SetDefaultData();
                // Don't register the persistable in order to acoid overwriting data.
            }
        }
    }
}