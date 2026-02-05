using System;
using UnityEngine;

namespace fireMCG.PathOfLayouts.LayoutBrowser.Ui
{
    public sealed class LayoutCard : MonoBehaviour
    {
        private Action<string> _settingsCallback;
        private Action<string> _playCallback;
        private string _layoutId;

        public void Initialize(Action<string> settingsCallback, Action<string> playCallback, string layoutId)
        {
            _settingsCallback = settingsCallback;
            _playCallback = playCallback;
            _layoutId = layoutId;
        }

        public void Play()
        {
            _playCallback?.Invoke(_layoutId);
        }

        public void OpenSettings()
        {
            _settingsCallback?.Invoke(_layoutId);
        }

        public void ToggleLayoutSrsState()
        {

        }
    }
}