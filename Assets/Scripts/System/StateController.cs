using fireMCG.PathOfLayouts.Messaging;
using UnityEngine;

namespace fireMCG.PathOfLayouts.System
{
    public sealed class StateController : MonoBehaviour
    {
        public enum AppState
        {
            Initializing,
            MainMenu,
            LayoutBrowser,
            LearningCenter,
            Gameplay
        }

        public static AppState PreviousState { get; private set; } = AppState.Initializing;
        public static AppState CurrentState { get; private set; } = AppState.Initializing;

        [field: SerializeField] private GameObject _mainMenuUiContainer;
        [field: SerializeField] private GameObject _layoutBrowserUiContainer;
        [field: SerializeField] private GameObject _learningCenterUiContainer;
        [field: SerializeField] private GameObject _gameplayUiContainer;

        private void Start()
        {
            RegisterMessageListeners();

            SetState(AppState.MainMenu);
        }

        private void OnDestroy()
        {
            UnregisterMessageListeners();
        }

        private void RegisterMessageListeners()
        {
            UnregisterMessageListeners();

            MessageBusManager.Resolve.Subscribe<OnAppStateChangeRequest>(OnAppStateChangeRequest);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Resolve.Unsubscribe<OnAppStateChangeRequest>(OnAppStateChangeRequest);
        }

        private void OnAppStateChangeRequest(OnAppStateChangeRequest message)
        {
            SetState(message.TargetState);

            OnAppStateChanged stateChangedMessage = new OnAppStateChanged(PreviousState, CurrentState);
            MessageBusManager.Resolve.Publish(stateChangedMessage);
        }

        private void SetState(AppState newState)
        {
            PreviousState = CurrentState;
            CurrentState = newState;

            _mainMenuUiContainer.SetActive(newState == AppState.MainMenu);
            _layoutBrowserUiContainer.SetActive(newState == AppState.LayoutBrowser);
            _learningCenterUiContainer.SetActive(newState == AppState.LearningCenter);
            _gameplayUiContainer.SetActive(newState == AppState.Gameplay);
        }
    }
}