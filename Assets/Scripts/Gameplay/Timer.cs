using fireMCG.PathOfLayouts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public class Timer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;

        private bool _isTimerOn = false;
        private float _timerInSeconds = 0f;

        private void Awake()
        {
            Assert.IsNotNull(_timerText);

            RestartTimer();
        }

        private void Update()
        {
            if(StateController.CurrentState != StateController.AppState.Gameplay)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                ToggleTimer();
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                RestartTimer();
            }

            if (!_isTimerOn)
            {
                return;
            }

            _timerInSeconds += Time.deltaTime;
            _timerText.text = GetTimerString();
        }

        public void SetTimerState(bool state)
        {
            _isTimerOn = state;
        }

        public void RestartTimer()
        {
            _isTimerOn = false;

            _timerInSeconds = 0f;
            _timerText.text = GetTimerString();
        }

        private void ToggleTimer()
        {
            SetTimerState(!_isTimerOn);
        }

        private string GetTimerString()
        {
            int minutes = Mathf.FloorToInt(_timerInSeconds / 60);
            int seconds = Mathf.FloorToInt(_timerInSeconds % 60);
            float decimals = _timerInSeconds - Mathf.FloorToInt(_timerInSeconds);

            return $"{minutes}:{seconds:D2}";
        }
    }
}