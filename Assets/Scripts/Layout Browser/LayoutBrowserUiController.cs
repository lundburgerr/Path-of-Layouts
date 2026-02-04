using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.System;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Ui
{
    public class LayoutBrowserUiController : MonoBehaviour
    {
        [SerializeField] private GameObject _actsMenuRoot;
        [SerializeField] private GameObject _gridViewRoot;
        [SerializeField] private GameObject _backButton;

        private void Awake()
        {
            ResetUi();
        }

        public void OpenMainMenu()
        {
            OnAppStateChangeRequest message = new OnAppStateChangeRequest(StateController.AppState.MainMenu);
            MessageBusManager.Resolve.Publish(message);

            ResetUi();
        }

        public void SetAct(string actId)
        {
            _actsMenuRoot.SetActive(false);
            _gridViewRoot.SetActive(true);
            _backButton.SetActive(true);
        }

        public void Back()
        {
            _actsMenuRoot.SetActive(true);
            _gridViewRoot.SetActive(false);
            _backButton.SetActive(false);
        }

        private void ResetUi()
        {
            _actsMenuRoot.SetActive(true);
            _gridViewRoot.SetActive(false);
            _backButton.SetActive(false);
        }
    }
}