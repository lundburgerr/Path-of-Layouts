using fireMCG.PathOfLayouts.Messaging;
using TMPro;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Prompt
{
    public class PromptUiController : MonoBehaviour
    {
        private const int MAX_MESSAGE_CHARACTER_COUNT = 512;

        [SerializeField] private GameObject _errorPromptContainer;
        [SerializeField] private TMP_Text _promptText;

        private void Awake()
        {
            RegisterMessageListeners();

            ClosePrompt();
        }

        private void OnDestroy()
        {
            UnregisterMessageListeners();
        }

        private void RegisterMessageListeners()
        {
            UnregisterMessageListeners();

            MessageBusManager.Resolve.Subscribe<OnErrorMessage>(OnError);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Resolve.Unsubscribe<OnErrorMessage>(OnError);
        }

        private void OnError(OnErrorMessage message)
        {
            _errorPromptContainer.SetActive(true);

            string errorMessage = message.ErrorMessage;
            if (errorMessage.Length > MAX_MESSAGE_CHARACTER_COUNT)
            {
                errorMessage = errorMessage.Substring(0, MAX_MESSAGE_CHARACTER_COUNT);
            }

            _promptText.text = errorMessage;
        }

        public void ClosePrompt()
        {
            _errorPromptContainer.SetActive(false);
            _promptText.text = string.Empty;
        }
    }
}