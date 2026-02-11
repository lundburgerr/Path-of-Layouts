using fireMCG.PathOfLayouts.Core;
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

            MessageBusManager.Resolve.Subscribe<OnAppStateChanged>(UpdateView);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Resolve.Unsubscribe<OnAppStateChanged>(UpdateView);
        }

        private void UpdateView(OnAppStateChanged message)
        {
            if(message.NewState != StateController.AppState.LearningCenter)
            {
                return;
            }

            FillDueWithinStatistics();
        }

        private  void FillDueWithinStatistics()
        {
            int totalDueLayouts = 0;
            int[] dueLayoutsAmount = new int[_dueWithinElements.Length];
            for(int i = 0; i < _dueWithinElements.Length; i++)
            {
                int tempValue = Bootstrap.Instance.SrsService.GetLayoutsDueWithin(_dueWithinElements[i].timeSpan);
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
    }
}