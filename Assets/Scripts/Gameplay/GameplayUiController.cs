using fireMCG.PathOfLayouts.Core;
using fireMCG.PathOfLayouts.Layouts;
using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.Srs;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public class GameplayUiController : MonoBehaviour
    {
        [SerializeField] private GameObject _gameplaySettings;
        [SerializeField] private TMP_InputField _movementSpeedField;
        [SerializeField] private TMP_InputField _lightRadiusField;
        [SerializeField] private Button _successButton;
        [SerializeField] private Button _failureButton;
        [SerializeField] private Button _randomReplayButton;
        [SerializeField] private TMP_Text _areaName;

        private void Awake()
        {
            Assert.IsNotNull(_gameplaySettings);
            Assert.IsNotNull(_movementSpeedField);
            Assert.IsNotNull(_lightRadiusField);
            Assert.IsNotNull(_successButton);
            Assert.IsNotNull(_failureButton);
            Assert.IsNotNull(_randomReplayButton);

            _gameplaySettings.SetActive(false);

            RegisterMessageListeners();
        }

        private void Start()
        {
            _movementSpeedField.text = PlayerPrefs.GetInt("movementSpeed").ToString();
            _lightRadiusField.text = PlayerPrefs.GetInt("lightRadius").ToString();
        }

        private void OnDestroy()
        {
            UnregisterMessageListeners();
        }

        private void RegisterMessageListeners()
        {
            UnregisterMessageListeners();

            MessageBusManager.Resolve.Subscribe<OnLayoutLoadedMessage>(OnLayoutLoaded);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Resolve.Unsubscribe<OnLayoutLoadedMessage>(OnLayoutLoaded);
        }

        private void OnLayoutLoaded(OnLayoutLoadedMessage message)
        {
            string srsEntryKey = SrsService.GetSrsEntryKey(message.ActId, message.AreaId, message.GraphId, message.LayoutId);
            SetSrsState(Bootstrap.Instance.SrsService.IsLayoutDue(srsEntryKey));

            _randomReplayButton.interactable = message.LayoutLoadingMethod != LayoutLoader.LayoutLoadingMethod.TargetLayout;

            _areaName.text = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(message.AreaId.Replace('_', ' '));
        }

        public void SetSrsState(bool enabled)
        {
            _successButton.interactable = enabled;
            _failureButton.interactable = enabled;
        }

        public void Replay()
        {
            MessageBusManager.Resolve.Publish(new OnReplayLayoutMessage(false));
        }

        public void RandomReplay()
        {
            MessageBusManager.Resolve.Publish(new OnReplayLayoutMessage(true));
        }

        public void ToggleGameplaySettings()
        {
            _gameplaySettings.SetActive(!_gameplaySettings.activeSelf);
        }

        public void ApplySettings()
        {
            if(!int.TryParse(_movementSpeedField.text, out int movementSpeedPercent))
            {
                Debug.LogError("GameplayUiContrller.ApplySettings error, parsing failed.");
            }

            if (!int.TryParse(_lightRadiusField.text, out int lightRadiusPercent))
            {
                Debug.LogError("GameplayUiContrller.ApplySettings error, parsing failed.");
            }

            PlayerPrefs.SetInt("movementSpeed", movementSpeedPercent);
            PlayerPrefs.SetInt("lightRadius", lightRadiusPercent);
            MessageBusManager.Resolve.Publish(new OnGameplaySettingsChangedMessage(movementSpeedPercent, lightRadiusPercent));
        }

        public void Quit()
        {
            MessageBusManager.Resolve.Publish(new OnAppStateChangeRequest(StateController.PreviousState));
        }

        public void RecordSrsSuccess()
        {
            MessageBusManager.Resolve.Publish(new RecordSrsResultMessage(SrsPracticeResult.Success));

            SetSrsState(false);
        }

        public void RecordSrsFailure()
        {
            MessageBusManager.Resolve.Publish(new RecordSrsResultMessage(SrsPracticeResult.Failure));

            SetSrsState(false);
        }
    }
}