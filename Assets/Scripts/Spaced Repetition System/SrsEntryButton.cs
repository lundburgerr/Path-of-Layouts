using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace fireMCG.PathOfLayouts.Srs
{
    public class SrsEntryButton : MonoBehaviour
    {
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _playButton;
        [SerializeField] private TMP_Text _label;

        private void Awake()
        {
            Assert.IsNotNull(_selectButton);
            Assert.IsNotNull(_playButton);
            Assert.IsNotNull(_label);
        }

        private void OnDestroy()
        {
            _selectButton.onClick.RemoveAllListeners();
            _playButton.onClick.RemoveAllListeners();
        }

        public void Initialize(Action<string> onSelect, Action<string> onPlay, string layoutId)
        {
            _label.text = layoutId;
            _selectButton.onClick.AddListener(() => onSelect?.Invoke(layoutId));
            _playButton.onClick.AddListener(() => onPlay?.Invoke(layoutId));
        }
    }
}