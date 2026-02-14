using fireMCG.PathOfLayouts.Messaging;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public class TimerUiController : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;

        // To do: Stop being lazy
        public static Timer timer;

        private void Awake()
        {
            Assert.IsNotNull(timerText);

            timer = new Timer();
            timerText.text = timer.ToString();
        }

        private void OnEnable()
        {
            RegisterMessageListeners();
        }

        private void OnDisable()
        {
            UnregisterMessageListeners();
        }

        private void Update()
        {
            // To do: Replace with new Input System.

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                timer.Toggle();
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                RestartTimer();
            }

            if (!timer.IsOn)
            {
                return;
            }

            timer.Tick(Time.deltaTime);
            timerText.text = timer.ToString();
        }

        private void RegisterMessageListeners()
        {
            MessageBusManager.Instance.Subscribe<StartTimerMessage>(OnStartTimerMessage);
            MessageBusManager.Instance.Subscribe<PauseTimerMessage>(OnPauseTimerMessage);
            MessageBusManager.Instance.Subscribe<RestartTimerMessage>(OnRestartTimerMessage);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Instance.Unsubscribe<StartTimerMessage>(OnStartTimerMessage);
            MessageBusManager.Instance.Unsubscribe<PauseTimerMessage>(OnPauseTimerMessage);
            MessageBusManager.Instance.Unsubscribe<RestartTimerMessage>(OnRestartTimerMessage);
        }

        private void OnStartTimerMessage(StartTimerMessage message) => timer.Start();

        private void OnPauseTimerMessage(PauseTimerMessage message) => timer.Pause();

        private void OnRestartTimerMessage(RestartTimerMessage message)
        {
            RestartTimer();
        }

        private void RestartTimer()
        {
            timer.Restart();
            timerText.text = timer.ToString();
        }
    }
}