using fireMCG.PathOfLayouts.Layouts;
using fireMCG.PathOfLayouts.Messaging;
using UnityEngine;
using UnityEngine.UI;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private RawImage _layoutDisplay;
        [SerializeField] private RectTransform _layoutTransform;

        [SerializeField] private CollisionMap _collisionMap;
        [SerializeField] private FogOfWar _fogOfWar;

        private void Start()
        {
            RegisterMessageListeners();
        }

        private void OnDestroy()
        {
            UnregisterMessageListeners();
        }

        private void RegisterMessageListeners()
        {
            UnregisterMessageListeners();

            MessageBusManager.Resolve.Subscribe<OnLayoutLoadedMessage>(OnLayoutLoaded);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Resolve.Unsubscribe<OnLayoutLoadedMessage>(OnLayoutLoaded);
        }

        private void OnLayoutLoaded(OnLayoutLoadedMessage message)
        {
            _layoutDisplay.texture = message.LayoutMap;
            _layoutTransform.sizeDelta = new Vector2(message.LayoutMap.width, message.LayoutMap.height);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutTransform);
        }
    }
}