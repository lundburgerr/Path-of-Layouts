using fireMCG.PathOfLayouts.Core;
using fireMCG.PathOfLayouts.Layouts;
using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.Ui.Components;
using System;
using System.Collections.Generic;
using TMPro;
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

        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _practicedText;
        [SerializeField] private TMP_Text _succeededText;
        [SerializeField] private TMP_Text _failedText;
        [SerializeField] private TMP_Text _successRateText;
        [SerializeField] private TMP_Text _averageTimeText;
        [SerializeField] private TMP_Text _bestTimeText;
        [SerializeField] private TMP_Text _nextPracticeText;
        [SerializeField] private TMP_Text _streakText;

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
            Assert.IsNotNull(_overviewDueContainer);
            Assert.IsNotNull(_overviewUpcomingContainer);
            Assert.IsNotNull(_overviewLowSuccessContainer);
            Assert.IsNotNull(_entryButtonPrefab);

            Assert.IsNotNull(_levelText);
            Assert.IsNotNull(_practicedText);
            Assert.IsNotNull(_succeededText);
            Assert.IsNotNull(_failedText);
            Assert.IsNotNull(_successRateText);
            Assert.IsNotNull(_averageTimeText);
            Assert.IsNotNull(_bestTimeText);
            Assert.IsNotNull(_nextPracticeText);
            Assert.IsNotNull(_streakText);

            Assert.IsNotNull(_ratioSlidersContainer);
            Assert.IsNotNull(_ratioSliderPrefab);
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
            MessageBusManager.Instance.Subscribe<OnAppStateChanged>(UpdateOverview);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Instance.Unsubscribe<OnAppStateChanged>(UpdateOverview);
        }

        public void QuitToMainMenu()
        {
            MessageBusManager.Instance.Publish(new OnAppStateChangeRequest(StateController.AppState.MainMenu));
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

            ClearUi();
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
                button.Initialize(
                    OnSelectEntry,
                    OnPlayEntry,
                    entry.graphId,
                    SrsService.GetSrsEntryKey(entry.actId, entry.areaId, entry.graphId, entry.layoutId));
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

        private void OnSelectEntry(string entryKey)
        {
            SrsLayoutData data = Bootstrap.Instance.SrsService.SrsData.layouts[entryKey];
            _levelText.text = data.masteryLevel.ToString();
            _practicedText.text = data.timesPracticed.ToString();
            _succeededText.text = data.timesSucceeded.ToString();
            _failedText.text = data.timesFailed.ToString();
            _successRateText.text = ((float)data.timesSucceeded / data.timesPracticed * 100).ToString("F0") + "%";

            // To do: Format the string properly using the timer string formatting
            _averageTimeText.text = data.averageTimeSeconds.ToString();
            _bestTimeText.text = data.bestTimeSeconds.ToString();

            // To do: Format due date to be short and easily readable
            _nextPracticeText.text = data.GetDueDateTime().ToString();

            // To do: Change streak label between "Success/Failure Streak" based on the value of data.lastResult
            _streakText.text = data.streak.ToString();
        }

        private void OnPlayEntry(string entryKey)
        {
            SrsLayoutData data = Bootstrap.Instance.SrsService.SrsData.layouts[entryKey]; 
            MessageBusManager.Instance.Publish(new LoadTargetLayoutMessage(data.actId, data.areaId, data.graphId, data.layoutId));
        }

        private void ClearUi()
        {
            _levelText.text = "N/A";
            _practicedText.text = "N/A";
            _succeededText.text = "N/A";
            _failedText.text = "N/A";
            _successRateText.text = "N/A";
            _averageTimeText.text = "N/A";
            _bestTimeText.text = "N/A";
            _nextPracticeText.text = "N/A";
            _streakText.text = "N/A";
        }
    }
}