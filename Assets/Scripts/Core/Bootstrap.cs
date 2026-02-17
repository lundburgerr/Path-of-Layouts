using fireMCG.PathOfLayouts.Campaign;
using fireMCG.PathOfLayouts.Content;
using fireMCG.PathOfLayouts.IO;
using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.Srs;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace fireMCG.PathOfLayouts.Core
{
    public sealed class Bootstrap : MonoBehaviour
    {
        public static Bootstrap Instance { get; private set; }

        [field: SerializeField] public CampaignDatabase CampaignDatabase { get; private set; }

        public ContentService ContentService { get; private set; }

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

            Assert.IsNotNull(CampaignDatabase);
        }

        private async void Start()
        {
            try
            {
                await InitializeAsync(_tokenSource.Token);

                IsReady = true;
                MessageBusManager.Instance.Publish(new OnBootstrapReadyMessage());
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

            if(ContentService != null)
            {
                ContentService.ReleaseAll();
            }

            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }

        private async Task InitializeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (CampaignDatabase == null)
            {
                throw new System.InvalidOperationException("Bootstrap.InitializeAsync error, CampaignDatabase is null.");
            }

            CampaignDatabase.BuildRuntimeIndex();
            ContentService = new ContentService(CampaignDatabase);

            SrsService = new SrsService();

            try
            {
                await SrsService.LoadSrsSaveDataAsync(token);
                MessageBusManager.Instance.Publish(new RegisterPersistableMessage(SrsService));
            }
            catch (System.OperationCanceledException)
            {
                throw;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Bootstrap.InitializeAsync error, Srs failed to load, continuing with defaults. e={e}");

                SrsService.SetDefaultData();
                // Don't register the persistable in order to avoid overwriting data.
            }
        }
    }
}