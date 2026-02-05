using fireMCG.PathOfLayouts.Manifest;
using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.System;
using System.Collections.Generic;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public sealed class GameplayController : MonoBehaviour
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

            MessageBusManager.Resolve.Subscribe<PlayRandomLayoutMessage>(PlayRandomLayout);
            MessageBusManager.Resolve.Subscribe<PlayTargetLayoutMessage>(PlayTargetLayout);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Resolve.Unsubscribe<PlayRandomLayoutMessage>(PlayRandomLayout);
            MessageBusManager.Resolve.Unsubscribe<PlayTargetLayoutMessage>(PlayTargetLayout);
        }

        private void PlayRandomLayout(PlayRandomLayoutMessage message)
        {
            IReadOnlyList<ActEntry> acts = Bootstrap.Instance.ManifestService.Manifest.acts;
            string actId = acts[Random.Range(0, acts.Count - 1)].actId;

            IReadOnlyList<AreaEntry> areas = Bootstrap.Instance.ManifestService.Manifest.GetAreas(actId);
            string areaId = areas[Random.Range(0, areas.Count - 1)].areaId; // placeholder

            IReadOnlyList<GraphEntry> graphs = Bootstrap.Instance.ManifestService.Manifest.GetGraphs(actId, areaId);
            string graphId = graphs[Random.Range(0, graphs.Count - 1)].graphId;

            string layoutId = "placeholder";

            PlayTargetLayout(actId, areaId, graphId, layoutId);
        }

        private void PlayTargetLayout(PlayTargetLayoutMessage message)
        {
            PlayTargetLayout(message.actId, message.areaId, message.graphId, message.layoutId);
        }

        private void PlayTargetLayout(string actId, string areaId, string graphId, string layoutId)
        {
            MessageBusManager.Resolve.Publish(new OnAppStateChangeRequest(StateController.AppState.Gameplay));
        }
    }
}