using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.System;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Ui
{
    public class MainMenuUiController : MonoBehaviour
    {
        [field: SerializeField] private GameObject _settingsWindow;

        private void Awake()
        {
            SetSettingsWindowState(false);
        }

        public void QuickPlay()
        {

        }

        public void OpenLayoutBrowser()
        {
            OnAppStateChangeRequest message = new OnAppStateChangeRequest(StateController.AppState.LayoutBrowser);
            MessageBusManager.Resolve.Publish(message);
        }

        public void OpenLearningCenter()
        {
            OnAppStateChangeRequest message = new OnAppStateChangeRequest(StateController.AppState.LearningCenter);
            MessageBusManager.Resolve.Publish(message);
        }

        public void ToggleSettingsWindow()
        {
            SetSettingsWindowState(!_settingsWindow.activeSelf);
        }

        public void Quit()
        {
            Application.Quit();
        }

        private void SetSettingsWindowState(bool state)
        {
            _settingsWindow.SetActive(state);
        }
    }
}