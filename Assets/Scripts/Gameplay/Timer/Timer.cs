using UnityEngine;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public class Timer
    {
        public float Time { get; private set; }
        public bool IsOn { get; private set; }

        public void Start() => ResetAndStart();

        public void Restart() => ResetAndStart();

        public void Pause() => IsOn = false;

        public void Resume() => IsOn = true;

        public void Toggle() => IsOn = !IsOn;

        private void ResetAndStart()
        {
            Time = 0f;
            IsOn = true;
        }

        public void Tick(float delta)
        {
            if (!IsOn)
            {
                return;
            }

            Time += delta;
        }

        // To do: Move formatting logic to string formatting script
        public override string ToString()
        {
            int totalSeconds = Mathf.FloorToInt(Time);

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            return $"{minutes}:{seconds:00}";
        }
    }
}