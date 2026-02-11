using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace fireMCG.PathOfLayouts.Ui.Components
{
    public class RatioSlider : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private RectTransform _elementsContainer;
        [SerializeField] private Image _elementPrefab;

        public void Initialize(string label, (int, Color)[] elements, int total)
        {
            float totalValue = 0;
            foreach((int value, Color color) element in elements)
            {
                totalValue += element.value;
                float ratio = (float)totalValue / total;
                Image image = Instantiate(_elementPrefab, _elementsContainer);
                image.color = element.color;
                image.rectTransform.sizeDelta = new Vector2(
                    _elementsContainer.sizeDelta.x * ratio,
                    0);

                image.rectTransform.SetAsFirstSibling();
            }

            _label.text = label;
            _amount.text = totalValue.ToString();
        }
    }
}