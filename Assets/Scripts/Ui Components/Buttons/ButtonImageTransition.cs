using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace fireMCG.PathOfLayouts.Ui.Components
{
    public class ButtonImageTransition : Button
    {
        [SerializeField] private GameObject _hoverOffIcon;
        [SerializeField] private GameObject _hoverOnIcon;

        protected override void Awake()
        {
            SetHoverState(false);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            SetHoverState(true);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            SetHoverState(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            SetHoverState(false);
        }

        private void SetHoverState(bool state)
        {
            if (_hoverOffIcon != null)
            {
                _hoverOffIcon.SetActive(!state);
            }

            if (_hoverOnIcon != null)
            {
                _hoverOnIcon.SetActive(state);
            }
        }
    }
}