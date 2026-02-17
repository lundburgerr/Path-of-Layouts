using UnityEngine.Assertions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using fireMCG.PathOfLayouts.Core;
using fireMCG.PathOfLayouts.Srs;

namespace fireMCG.PathOfLayouts.LayoutBrowser.Ui
{
    public sealed class LayoutCard : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private RectTransform _thumbnailBackground;
        [SerializeField] private RectTransform _thumbnailContainer;
        [SerializeField] private RawImage _thumbnailImage;
        [SerializeField] private GameObject _addToLearningButton;
        [SerializeField] private GameObject _removeFromLearningButton;

        private Action<string> _settingsCallback;
        private Action<string> _playCallback;
        private string _layoutId;

        private bool _isLearning;

        private void Awake()
        {
            Assert.IsNotNull(_label);
            Assert.IsNotNull(_thumbnailBackground);
            Assert.IsNotNull(_thumbnailContainer);
            Assert.IsNotNull(_thumbnailImage);
            Assert.IsNotNull(_addToLearningButton);
            Assert.IsNotNull(_removeFromLearningButton);
        }

        public void Initialize(Action<string> settingsCallback, Action<string> playCallback, string layoutId, string displayName)
        {
            _layoutId = layoutId;

            _label.text = displayName;

            _thumbnailImage.texture = null;

            _settingsCallback = settingsCallback;
            _playCallback = playCallback;

            _isLearning = Bootstrap.Instance.SrsService.IsLearning(layoutId);

            SetSrsButtonStates();
        }

        public void SetThumbnail(Texture2D thumbnail)
        {
            if(thumbnail == null)
            {
                _thumbnailImage.texture = null;

                return;
            }

            float scaleX = _thumbnailBackground.sizeDelta.x / thumbnail.width;
            float scaleY = _thumbnailBackground.sizeDelta.y / thumbnail.height;
            float scale = Mathf.Min(scaleX, scaleY);

            _thumbnailContainer.sizeDelta = new Vector2(thumbnail.width * scale, thumbnail.height * scale);
            _thumbnailImage.texture = thumbnail;
        }

        public void Play()
        {
            _playCallback?.Invoke(_layoutId);
        }

        public void OpenSettings()
        {
            _settingsCallback?.Invoke(_layoutId);
        }

        public void AddToLearning()
        {
            if (!Bootstrap.Instance.SrsService.AddToLearning(_layoutId))
            {
                return;
            }

            _isLearning = true;

            SetSrsButtonStates();
        }

        public void RemoveFromLearning()
        {
            if (!Bootstrap.Instance.SrsService.RemoveFromLearning(_layoutId))
            {
                return;
            }

            _isLearning = false;

            SetSrsButtonStates();
        }

        private void SetSrsButtonStates()
        {
            _addToLearningButton.SetActive(!_isLearning);
            _removeFromLearningButton.SetActive(_isLearning);
        }
    }
}