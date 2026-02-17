using fireMCG.PathOfLayouts.Common;
using fireMCG.PathOfLayouts.Core;
using fireMCG.PathOfLayouts.Layouts;
using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.Ui.Components;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace fireMCG.PathOfLayouts.Srs
{
    /// <summary>
    /// To do: Split views into their own scripts and make this script the navigation controller.
    /// To do: Replace instantiation and destroy logic with an object pool.
    /// To do: Implement limits in the views (other than overview which already has it) to prevent overflow
    ///     and implement pagination.
    /// To do: Change streak label between "Success/Failure Streak" based on the value of data.lastResult.
    /// </summary>
    public sealed class SrsUiController : MonoBehaviour
    {
        private const int MAX_OVERVIEW_CONTAINER_ENTRIES = 13;

        [SerializeField] private GameObject _overviewView;
        [SerializeField] private RectTransform _overviewDueContainer;
        [SerializeField] private RectTransform _overviewUpcomingContainer;
        [SerializeField] private RectTransform _overviewLowSuccessContainer;
        [SerializeField] private SrsEntryButton _entryButtonPrefab;

        [SerializeField] private GameObject _dueView;
        [SerializeField] private RectTransform _dueContainer;

        [SerializeField] private GameObject _upcomingView;
        [SerializeField] private RectTransform _upcomingContainer;

        [SerializeField] private GameObject _disabledView;
        [SerializeField] private RectTransform _disabledContainer;

        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _practicedText;
        [SerializeField] private TMP_Text _succeededText;
        [SerializeField] private TMP_Text _failedText;
        [SerializeField] private TMP_Text _successRateText;
        [SerializeField] private TMP_Text _averageTimeText;
        [SerializeField] private TMP_Text _bestTimeText;
        [SerializeField] private TMP_Text _nextPracticeText;
        [SerializeField] private TMP_Text _streakText;

        [SerializeField] private TMP_Text _toggleLearningText;
        [SerializeField] private Image _toggleLearningImage;
        [SerializeField] private Button _toggleLearningButton;

        [SerializeField] private RectTransform _ratioSlidersContainer;
        [SerializeField] private RatioSlider _ratioSliderPrefab;

        private string _selectedEntryId = string.Empty;

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

            Assert.IsNotNull(_toggleLearningText);
            Assert.IsNotNull(_toggleLearningImage);
            Assert.IsNotNull(_toggleLearningButton);

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

            ShowOverview();
        }

        public void ShowOverview()
        {
            ClearStatisticsUi();

            _overviewView.SetActive(true);
            _dueView.SetActive(false);
            _upcomingView.SetActive(false);
            _disabledView.SetActive(false);

            FillViewContainer(_overviewDueContainer, Bootstrap.Instance.SrsService.GetDueLayouts(MAX_OVERVIEW_CONTAINER_ENTRIES));
            FillViewContainer(_overviewUpcomingContainer, Bootstrap.Instance.SrsService.GetNextDueLayouts(MAX_OVERVIEW_CONTAINER_ENTRIES));
            FillViewContainer(_overviewLowSuccessContainer, Bootstrap.Instance.SrsService.GetLowSuccessLayouts(MAX_OVERVIEW_CONTAINER_ENTRIES));
            FillDueWithinStatistics();
        }

        public void ShowDue()
        {
            ClearStatisticsUi();

            _overviewView.SetActive(false);
            _dueView.SetActive(true);
            _upcomingView.SetActive(false);
            _disabledView.SetActive(false);

            FillViewContainer(_dueContainer, Bootstrap.Instance.SrsService.GetDueLayouts());
        }

        public void ShowUpcoming()
        {
            ClearStatisticsUi();

            _overviewView.SetActive(false);
            _dueView.SetActive(false);
            _upcomingView.SetActive(true);
            _disabledView.SetActive(false);

            FillViewContainer(_upcomingContainer, Bootstrap.Instance.SrsService.GetNextDueLayouts());
        }

        public void ShowDisabled()
        {
            ClearStatisticsUi();

            _overviewView.SetActive(false);
            _dueView.SetActive(false);
            _upcomingView.SetActive(false);
            _disabledView.SetActive(true);

            FillViewContainer(_disabledContainer, Bootstrap.Instance.SrsService.GetDisabledLayouts());
        }

        private void FillViewContainer(RectTransform container, IReadOnlyList<SrsLayoutData> entries)
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
            _selectedEntryId = entryKey;
            SrsLayoutData data = Bootstrap.Instance.SrsService.SrsData.layouts[_selectedEntryId];

            _levelText.text = data.masteryLevel.ToString();
            _practicedText.text = data.timesPracticed.ToString();
            _succeededText.text = data.timesSucceeded.ToString();
            _failedText.text = data.timesFailed.ToString();
            _successRateText.text = ((float)data.timesSucceeded / data.timesPracticed * 100).ToString("F0") + "%";

            _averageTimeText.text = TimeFormatter.FormatTimeExplicit(data.averageTimeSeconds);
            _bestTimeText.text = TimeFormatter.FormatTimeExplicit(data.bestTimeSeconds);

            _nextPracticeText.text = data.GetTimeStringUntilDue(DateTime.UtcNow);

            _streakText.text = data.streak.ToString();

            SetEntryLearningStateUi();
        }

        public void ToggleEntryLearningState()
        {
            if (!Bootstrap.Instance.SrsService.ToggleLearningState(_selectedEntryId))
            {
                ClearLearningStateUi();

                return;
            }

            
            SetEntryLearningStateUi();
        }

        private void SetEntryLearningStateUi()
        {
            SrsLayoutData data = Bootstrap.Instance.SrsService.SrsData.layouts[_selectedEntryId];
            if (data is null)
            {
                ClearLearningStateUi();

                return;
            }

            _toggleLearningButton.interactable = true;
            _toggleLearningImage.color = data.isLearning ? new Color(150f, 0f, 0f) : new Color(0f, 150f, 0f);
            _toggleLearningText.text = data.isLearning ? "Disable" : "Enable";
        }

        private void ClearLearningStateUi()
        {
            _toggleLearningImage.color = Color.white;
            _toggleLearningButton.interactable = false;
            _toggleLearningText.text = "N/A";
        }

        private void OnPlayEntry(string entryKey)
        {
            SrsLayoutData data = Bootstrap.Instance.SrsService.SrsData.layouts[entryKey]; 
            MessageBusManager.Instance.Publish(new LoadTargetLayoutMessage(data.layoutId));
        }

        private void ClearStatisticsUi()
        {
            _selectedEntryId = string.Empty;
            ClearLearningStateUi();

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