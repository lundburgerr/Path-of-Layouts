using fireMCG.PathOfLayouts.Campaign;
using fireMCG.PathOfLayouts.Content;
using fireMCG.PathOfLayouts.Core;
using fireMCG.PathOfLayouts.IO;
using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.Prompt;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Layouts
{
    public sealed class LayoutLoader : MonoBehaviour
    {
        public enum LayoutLoadingMethod { RandomAct, RandomArea, RandomGraph, RandomLayout, TargetLayout }

        private CancellationTokenSource _loadTokenSource;

        private void OnDestroy()
        {
            _loadTokenSource.Cancel();
            _loadTokenSource?.Dispose();
        }

        private void OnEnable()
        {
            RegisterMessageListeners();
        }

        private void OnDisable()
        {
            UnregisterMessageListeners();
        }

        private void RegisterMessageListeners()
        {
            MessageBusManager.Instance.Subscribe<LoadRandomActMessage>(OnPlayRandomAct);
            MessageBusManager.Instance.Subscribe<LoadRandomAreaMessage>(OnPlayRandomArea);
            MessageBusManager.Instance.Subscribe<LoadRandomGraphMessage>(OnPlayRandomGraph);
            MessageBusManager.Instance.Subscribe<LoadRandomLayoutMessage>(OnPlayRandomLayout);
            MessageBusManager.Instance.Subscribe<LoadTargetLayoutMessage>(OnPlayTargetLayout);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Instance.Unsubscribe<LoadRandomActMessage>(OnPlayRandomAct);
            MessageBusManager.Instance.Unsubscribe<LoadRandomAreaMessage>(OnPlayRandomArea);
            MessageBusManager.Instance.Unsubscribe<LoadRandomGraphMessage>(OnPlayRandomGraph);
            MessageBusManager.Instance.Unsubscribe<LoadRandomLayoutMessage>(OnPlayRandomLayout);
            MessageBusManager.Instance.Unsubscribe<LoadTargetLayoutMessage>(OnPlayTargetLayout);
        }

        private void OnPlayRandomAct(LoadRandomActMessage message)
        {
            IReadOnlyList<ActDef> acts = Bootstrap.Instance.CampaignDatabase.acts;
            string actId = acts[Random.Range(0, acts.Count)].id;

            PlayRandomAreaInternal(actId, null, LayoutLoadingMethod.RandomAct);
        }

        private void OnPlayRandomArea(LoadRandomAreaMessage message)
        {
            PlayRandomAreaInternal(message.ActId, message.ActId, LayoutLoadingMethod.RandomAct);
        }

        private void OnPlayRandomGraph(LoadRandomGraphMessage message)
        {
            PlayRandomGraphInternal(message.AreaId, message.AreaId, LayoutLoadingMethod.RandomGraph);
        }

        private void OnPlayRandomLayout(LoadRandomLayoutMessage message)
        {
            PlayRandomLayoutInternal(message.GraphId, message.GraphId, LayoutLoadingMethod.RandomLayout);
        }

        private void PlayRandomAreaInternal(string actId, string rootId, LayoutLoadingMethod loadingMethod)
        {
            IReadOnlyList<AreaDef> areas = Bootstrap.Instance.CampaignDatabase.GetAct(actId).areas;
            string areaId = areas[Random.Range(0, areas.Count)].id;

            PlayRandomGraphInternal(areaId, rootId, LayoutLoadingMethod.RandomArea);
        }

        private void PlayRandomGraphInternal(string areaId, string rootId, LayoutLoadingMethod loadingMethod)
        {
            IReadOnlyList<GraphDef> graphs = Bootstrap.Instance.CampaignDatabase.GetArea(areaId).graphs;
            string graphId = graphs[Random.Range(0, graphs.Count)].id;

            PlayRandomLayoutInternal(graphId, rootId, LayoutLoadingMethod.RandomGraph);
        }

        private void PlayRandomLayoutInternal(string graphId, string rootId, LayoutLoadingMethod loadingMethod)
        {
            IReadOnlyList<LayoutDef> layouts = Bootstrap.Instance.CampaignDatabase.GetGraph(graphId).layouts;
            string layoutId = layouts[Random.Range(0, layouts.Count)].id;

            _ = LoadLayoutAsync(layoutId, rootId, loadingMethod);
        }

        private void OnPlayTargetLayout(LoadTargetLayoutMessage message)
        {
            _ = LoadLayoutAsync(message.LayoutId, null, LayoutLoadingMethod.TargetLayout);
        }

        private async Task LoadLayoutAsync(string layoutId, string rootId, LayoutLoadingMethod loadingMethod)
        {
            if (string.IsNullOrWhiteSpace(layoutId))
            {
                MessageBusManager.Instance.Publish(new OnErrorMessage("Failed to load layout: layoutId is empty."));
                MessageBusManager.Instance.Publish(new OnAppStateChangeRequest(StateController.AppState.MainMenu));

                return;
            }

            CancelAndRenewLoadTokenSource();

            CancellationToken token = _loadTokenSource.Token;

            if (Bootstrap.Instance.ContentService == null)
            {
                MessageBusManager.Instance.Publish(new OnErrorMessage("Failed to load layout: ContentService is not initialized."));

                return;
            }

            try
            {
                token.ThrowIfCancellationRequested();

                Texture2D layoutImage = await Bootstrap.Instance.ContentService.LoadLayoutImageAsync(layoutId, token);

                token.ThrowIfCancellationRequested();

                MessageBusManager.Instance.Publish(new OnAppStateChangeRequest(StateController.AppState.Gameplay));
                MessageBusManager.Instance.Publish(new OnLayoutLoadedMessage(
                    rootId,
                    layoutId,
                    layoutImage,
                    loadingMethod));
            }
            catch (System.OperationCanceledException) { }
            catch (System.Exception e)
            {
                Debug.LogError(e);

                MessageBusManager.Instance.Publish(new OnErrorMessage("Failed to load layout image."));
            }
        }

        private void CancelAndRenewLoadTokenSource()
        {
            if(_loadTokenSource is not null)
            {
                _loadTokenSource.Cancel();
                _loadTokenSource.Dispose();
            }

            _loadTokenSource = new CancellationTokenSource();
        }
    }
}