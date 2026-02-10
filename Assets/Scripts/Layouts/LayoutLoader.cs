using fireMCG.PathOfLayouts.Common;
using fireMCG.PathOfLayouts.Core;
using fireMCG.PathOfLayouts.IO;
using fireMCG.PathOfLayouts.Manifest;
using fireMCG.PathOfLayouts.Messaging;
using System.Collections.Generic;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Layouts
{
    public class LayoutLoader : MonoBehaviour
    {
        private void Awake()
        {
            RegisterMessageListeners();
        }

        private void OnDestroy()
        {
            UnregisterMessageListeners();
        }

        private void RegisterMessageListeners()
        {
            UnregisterMessageListeners();

            MessageBusManager.Resolve.Subscribe<LoadRandomActMessage>(PlayRandomAct);
            MessageBusManager.Resolve.Subscribe<LoadRandomAreaMessage>(PlayRandomArea);
            MessageBusManager.Resolve.Subscribe<LoadRandomGraphMessage>(PlayRandomGraph);
            MessageBusManager.Resolve.Subscribe<LoadRandomLayoutMessage>(PlayRandomLayout);
            MessageBusManager.Resolve.Subscribe<LoadTargetLayoutMessage>(PlayTargetLayout);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Resolve.Unsubscribe<LoadRandomActMessage>(PlayRandomAct);
            MessageBusManager.Resolve.Unsubscribe<LoadRandomAreaMessage>(PlayRandomArea);
            MessageBusManager.Resolve.Unsubscribe<LoadRandomGraphMessage>(PlayRandomGraph);
            MessageBusManager.Resolve.Unsubscribe<LoadRandomLayoutMessage>(PlayRandomLayout);
            MessageBusManager.Resolve.Unsubscribe<LoadTargetLayoutMessage>(PlayTargetLayout);
        }

        private void PlayRandomAct(LoadRandomActMessage message)
        {
            IReadOnlyList<ActEntry> acts = Bootstrap.Instance.ManifestService.Manifest.acts;
            string actId = acts[Random.Range(0, acts.Count)].actId;

            PlayRandomArea(actId);
        }

        private void PlayRandomArea(LoadRandomAreaMessage message)
        {
            IReadOnlyList<AreaEntry> areas = Bootstrap.Instance.ManifestService.Manifest.GetAreas(message.ActId);
            string areaId = areas[Random.Range(0, areas.Count)].areaId;

            PlayRandomGraph(message.ActId, areaId);
        }

        private void PlayRandomArea(string actId)
        {
            IReadOnlyList<AreaEntry> areas = Bootstrap.Instance.ManifestService.Manifest.GetAreas(actId);
            string areaId = areas[Random.Range(0, areas.Count)].areaId;

            PlayRandomGraph(actId, areaId);
        }

        private void PlayRandomGraph(LoadRandomGraphMessage message)
        {
            IReadOnlyList<GraphEntry> graphs = Bootstrap.Instance.ManifestService.Manifest.GetGraphs(message.ActId, message.AreaId);
            string graphId = graphs[Random.Range(0, graphs.Count)].graphId;

            PlayRandomLayout(message.ActId, message.AreaId, graphId);
        }

        private void PlayRandomGraph(string actId, string areaId)
        {
            IReadOnlyList<GraphEntry> graphs = Bootstrap.Instance.ManifestService.Manifest.GetGraphs(actId, areaId);
            string graphId = graphs[Random.Range(0, graphs.Count)].graphId;

            PlayRandomLayout(actId, areaId, graphId);
        }

        private void PlayRandomLayout(LoadRandomLayoutMessage message)
        {
            IReadOnlyList<string> layouts = Bootstrap.Instance.ManifestService.Manifest.GetLayoutIds(message.ActId, message.AreaId, message.GraphId);
            string layoutId = layouts[Random.Range(0, layouts.Count)];

            TryLoadLayout(message.ActId, message.AreaId, message.GraphId, layoutId);
        }

        private void PlayRandomLayout(string actId, string areaId, string graphId)
        {
            IReadOnlyList<string> layouts = Bootstrap.Instance.ManifestService.Manifest.GetLayoutIds(actId, areaId, graphId);
            string layoutId = layouts[Random.Range(0, layouts.Count)];

            TryLoadLayout(actId, areaId, graphId, layoutId);
        }

        private void PlayTargetLayout(LoadTargetLayoutMessage message)
        {
            TryLoadLayout(message.ActId, message.AreaId, message.GraphId, message.LayoutId);
        }

        private void TryLoadLayout(string actId, string areaId, string graphId, string layoutId)
        {
            Texture2D layoutMap = null;
            Texture2D collisionMap = null;
            string layoutPath = StreamingPathResolver.GetLayoutFilePath(actId, areaId, graphId, layoutId);
            string collisionPath = StreamingPathResolver.GetCollisionMapFilePath(actId, areaId, graphId, layoutId);

            try
            {
                layoutMap = TextureFileLoader.LoadPng(layoutPath, FilterMode.Bilinear);
                collisionMap = TextureFileLoader.LoadPng(collisionPath, FilterMode.Point);

                if(layoutMap is null || collisionMap is null)
                {
                    throw new System.Exception("Layout and collision maps can't be null.");
                }
            }
            catch(System.Exception e)
            {
                throw new System.Exception($"LayoutLoader.TryLoadLayout failed to load textures.", e);
            }

            MessageBusManager.Resolve.Publish(new OnAppStateChangeRequest(StateController.AppState.Gameplay));

            OnLayoutLoadedMessage message = new OnLayoutLoadedMessage(
                actId,
                areaId,
                graphId,
                layoutId,
                layoutMap,
                collisionMap);
            MessageBusManager.Resolve.Publish(message);
        }
    }
}