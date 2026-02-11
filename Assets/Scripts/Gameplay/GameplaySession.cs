using fireMCG.PathOfLayouts.Layouts;
using fireMCG.PathOfLayouts.Messaging;
using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.UI;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public class GameplaySession : MonoBehaviour
    {
        [SerializeField] private RawImage _layoutDisplay;
        [SerializeField] private RectTransform _layoutTransform;

        [SerializeField] private PlayerController _playerController;
        [SerializeField] private CollisionMap _collisionMap;
        [SerializeField] private FogOfWar _fogOfWar;
        [SerializeField] private Timer _timer;

        private OnLayoutLoadedMessage _cachedLayoutMessage;

        private void Awake()
        {
            Assert.IsNotNull(_layoutDisplay);
            Assert.IsNotNull(_layoutTransform);
            Assert.IsNotNull(_playerController);
            Assert.IsNotNull(_collisionMap);
            Assert.IsNotNull(_fogOfWar);

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
            MessageBusManager.Resolve.Subscribe<OnReplayLayoutMessage>(OnReplayLayout);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Resolve.Unsubscribe<OnLayoutLoadedMessage>(OnLayoutLoaded);
            MessageBusManager.Resolve.Unsubscribe<OnReplayLayoutMessage>(OnReplayLayout);
        }

        private void OnLayoutLoaded(OnLayoutLoadedMessage message)
        {
            _layoutDisplay.texture = message.LayoutMap;
            _layoutTransform.sizeDelta = new Vector2(message.LayoutMap.width, message.LayoutMap.height);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutTransform);

            _collisionMap.Build(message.CollisionMap);
            _fogOfWar.Build(message.LayoutMap.width, message.LayoutMap.height);
            _playerController.Initialize();
            _timer.RestartTimer();

            _cachedLayoutMessage = message;
        }

        public void OnReplayLayout(OnReplayLayoutMessage message)
        {
            if (!message.IsRandom)
            {
                Replay();

                return;
            }

            switch (_cachedLayoutMessage.LayoutLoadingMethod)
            {
                case LayoutLoader.LayoutLoadingMethod.RandomAct:
                    MessageBusManager.Resolve.Publish(
                        new LoadRandomActMessage());
                    break;
                case LayoutLoader.LayoutLoadingMethod.RandomArea:
                    MessageBusManager.Resolve.Publish(
                        new LoadRandomAreaMessage(
                            _cachedLayoutMessage.ActId));
                    break;
                case LayoutLoader.LayoutLoadingMethod.RandomGraph:
                    MessageBusManager.Resolve.Publish(
                        new LoadRandomGraphMessage(
                            _cachedLayoutMessage.ActId,
                            _cachedLayoutMessage.AreaId));
                    break;
                case LayoutLoader.LayoutLoadingMethod.RandomLayout:
                    MessageBusManager.Resolve.Publish(
                        new LoadRandomLayoutMessage(
                            _cachedLayoutMessage.ActId,
                            _cachedLayoutMessage.AreaId,
                            _cachedLayoutMessage.GraphId));
                    break;
                default:
                    Replay();
                    break;
            }
        }

        private void Replay()
        {
            _fogOfWar.Build(_layoutDisplay.texture.width, _layoutDisplay.texture.height);
            _timer.RestartTimer();
            _playerController.Initialize();
        }
    }
}