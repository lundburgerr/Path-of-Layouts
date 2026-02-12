using fireMCG.PathOfLayouts.Core;
using fireMCG.PathOfLayouts.Layouts;
using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.Ui.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace fireMCG.PathOfLayouts.Srs
{
    public sealed class SrsUiController : MonoBehaviour
    {
        private const int MAX_OVERVIEW_CONTAINER_ENTRIES = 13;

        [SerializeField] private RectTransform _overviewDueContainer;
        [SerializeField] private RectTransform _overviewUpcomingContainer;
        [SerializeField] private RectTransform _overviewLowSuccessContainer;
        [SerializeField] private SrsEntryButton _entryButtonPrefab;

        [SerializeField] private RectTransform _ratioSlidersContainer;
        [SerializeField] private RatioSlider _ratioSliderPrefab;

        private static (string label, TimeSpan timeSpan, Color color)[] _dueWithinElements =
        {
            ("6 Hours", TimeSpan.FromHours(6), Color.red),
            ("1 Day", TimeSpan.FromDays(1), Color.orange),
            ("3 Days", TimeSpan.FromDays(3), Color.yellow),
            ("1 Week", TimeSpan.FromDays(7), Color.green),
            ("2 Weeks", TimeSpan.FromDays(14), Color.turquoise),
            ("1 Month", TimeSpan.FromDays(30), Color.cyan)
        };

        private void Awake()
        {
            Assert.IsNotNull(_ratioSliderPrefab);

            RegisterMessageListeners();
        }

        private void OnDestroy()
        {
            UnregisterMessageListeners();
        }

        private void RegisterMessageListeners()
        {
            UnregisterMessageListeners();

            MessageBusManager.Resolve.Subscribe<OnAppStateChanged>(UpdateOverview);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Resolve.Unsubscribe<OnAppStateChanged>(UpdateOverview);
        }

        public void QuitToMainMenu()
        {
            MessageBusManager.Resolve.Publish(new OnAppStateChangeRequest(StateController.AppState.MainMenu));
        }

        private void UpdateOverview(OnAppStateChanged message)
        {
            if(message.NewState != StateController.AppState.LearningCenter)
            {
                return;
            }

            FillOverviewContainer(_overviewDueContainer, Bootstrap.Instance.SrsService.GetDueLayouts(MAX_OVERVIEW_CONTAINER_ENTRIES));
            FillOverviewContainer(_overviewUpcomingContainer, Bootstrap.Instance.SrsService.GetNextDueLayouts(MAX_OVERVIEW_CONTAINER_ENTRIES));
            FillOverviewContainer(_overviewLowSuccessContainer, Bootstrap.Instance.SrsService.GetLowSuccessLayouts(MAX_OVERVIEW_CONTAINER_ENTRIES));
            FillDueWithinStatistics();
        }

        private void FillOverviewContainer(RectTransform container, IReadOnlyList<SrsLayoutData> entries)
        {
            foreach(Transform transform in container)
            {
                Destroy(transform.gameObject);
            }

            foreach(SrsLayoutData entry in entries)
            {
                SrsEntryButton button = Instantiate(_entryButtonPrefab, container);
                button.SetLabel(entry.graphId);
                button.SetOnClickListener(OnEntrySelected, SrsService.GetSrsEntryKey(entry.actId, entry.areaId, entry.graphId, entry.layoutId));
            }
        }

        private void FillDueWithinStatistics()
        {
            foreach (Transform transform in _ratioSlidersContainer)
            {
                Destroy(transform.gameObject);
            }

            int totalDueLayouts = 0;
            int[] dueLayoutsAmount = new int[_dueWithinElements.Length];
            for(int i = 0; i < _dueWithinElements.Length; i++)
            {
                DateTime dueAfter = i > 0 ? DateTime.UtcNow.Add(_dueWithinElements[i - 1].timeSpan) : DateTime.MinValue;
                int tempValue = Bootstrap.Instance.SrsService.GetLayoutsDueWithin(dueAfter, _dueWithinElements[i].timeSpan);
                dueLayoutsAmount[i] = tempValue;
                totalDueLayouts += tempValue;
            }

            List<(int dueAmount, Color color)> elements = new();
            for(int i = 0; i < dueLayoutsAmount.Length; i++)
            {
                elements.Add((dueLayoutsAmount[i], _dueWithinElements[i].color));
                RatioSlider ratioSlider = Instantiate(_ratioSliderPrefab, _ratioSlidersContainer);
                ratioSlider.Initialize(_dueWithinElements[i].label, elements.ToArray(), totalDueLayouts);
            }
        }

        private void OnEntrySelected(string entryKey)
        {
            SrsLayoutData data = Bootstrap.Instance.SrsService.SrsData.layouts[entryKey]; 
            MessageBusManager.Resolve.Publish(new LoadTargetLayoutMessage(data.actId, data.areaId, data.graphId, data.layoutId));
        }
    }
}