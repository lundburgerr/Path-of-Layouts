using UnityEngine.Assertions;
using System;
using TMPro;
using UnityEngine;

namespace fireMCG.PathOfLayouts.LayoutBrowser.Ui
{
    public sealed class AreaCard : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        private Action<string> _selectedCallback;
        private Action<string> _playCallback;
        private string _areaId;

        private void Awake()
        {
            Assert.IsNotNull(_label);
        }

        public void Initialize(Action<string> selectedCallback, Action<string> playCallback, string areaId, string displayName)
        {
            _label.text = displayName;

            _selectedCallback = selectedCallback;
            _playCallback = playCallback;
            _areaId = areaId;
        }

        public void Select()
        {
            _selectedCallback?.Invoke(_areaId);
        }

        public void Play()
        {
            _playCallback?.Invoke(_areaId);
        }
    }
}