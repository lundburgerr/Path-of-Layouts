using fireMCG.PathOfLayouts.Core;
using fireMCG.PathOfLayouts.Layouts;
using fireMCG.PathOfLayouts.Messaging;
using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.UI;
using fireMCG.PathOfLayouts.Srs;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public class GameplaySession : MonoBehaviour
    {
        public struct ReplayContext
        {
            public readonly string ActId;
            public readonly string AreaId;
            public readonly string GraphId;
            public readonly string LayoutId;
            public readonly LayoutLoader.LayoutLoadingMethod LayoutLoadingMethod;

            public ReplayContext(string actId, string areaId, string graphId, string layoutId, LayoutLoader.LayoutLoadingMethod method)
            {
                ActId = actId;
                AreaId = areaId;
                GraphId = graphId;
                LayoutId = layoutId;
                LayoutLoadingMethod = method;
            }

            public ReplayContext(OnLayoutLoadedMessage message)
            {
                ActId = message.ActId;
                AreaId = message.AreaId;
                GraphId = message.GraphId;
                LayoutId = message.LayoutId;
                LayoutLoadingMethod = message.LayoutLoadingMethod;
            }
        }

        [SerializeField] private RawImage _layoutDisplay;
        [SerializeField] private RectTransform _layoutTransform;

        [SerializeField] private PlayerController _playerController;
        [SerializeField] private CollisionMap _collisionMap;
        [SerializeField] private FogOfWar _fogOfWar;

        private ReplayContext _replayContext;

        private void Awake()
        {
            Assert.IsNotNull(_layoutDisplay);
            Assert.IsNotNull(_layoutTransform);
            Assert.IsNotNull(_playerController);
            Assert.IsNotNull(_collisionMap);
            Assert.IsNotNull(_fogOfWar);
        }

        private void OnEnable()
        {
            RegisterMessageListeners();
        }

        private void OnDisable()
        {
            UnregisterMessageListeners();
        }

        private void RegisterMessageListeners()
        {
            MessageBusManager.Instance.Subscribe<OnLayoutLoadedMessage>(OnLayoutLoaded);
            MessageBusManager.Instance.Subscribe<OnReplayLayoutMessage>(OnReplayLayout);
            MessageBusManager.Instance.Subscribe<RecordSrsResultMessage>(RecordSrsResult);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Instance.Unsubscribe<OnLayoutLoadedMessage>(OnLayoutLoaded);
            MessageBusManager.Instance.Unsubscribe<OnReplayLayoutMessage>(OnReplayLayout);
            MessageBusManager.Instance.Unsubscribe<RecordSrsResultMessage>(RecordSrsResult);
        }

        private void OnLayoutLoaded(OnLayoutLoadedMessage message)
        {
            _layoutDisplay.texture = message.LayoutMap;
            _layoutTransform.sizeDelta = new Vector2(message.LayoutMap.width, message.LayoutMap.height);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutTransform);

            _collisionMap.Build(message.CollisionMap);
            _fogOfWar.Build(message.LayoutMap.width, message.LayoutMap.height);
            _playerController.Initialize();
            MessageBusManager.Instance.Publish(new RestartTimerMessage());

            _replayContext = new ReplayContext(message);
        }

        public void OnReplayLayout(OnReplayLayoutMessage message)
        {
            if (!message.IsRandom || _replayContext.LayoutLoadingMethod == LayoutLoader.LayoutLoadingMethod.TargetLayout)
            {
                Replay();

                return;
            }

            switch (_replayContext.LayoutLoadingMethod)
            {
                case LayoutLoader.LayoutLoadingMethod.RandomAct:
                    MessageBusManager.Instance.Publish(
                        new LoadRandomActMessage());
                    break;
                case LayoutLoader.LayoutLoadingMethod.RandomArea:
                    MessageBusManager.Instance.Publish(
                        new LoadRandomAreaMessage(
                            _replayContext.ActId));
                    break;
                case LayoutLoader.LayoutLoadingMethod.RandomGraph:
                    MessageBusManager.Instance.Publish(
                        new LoadRandomGraphMessage(
                            _replayContext.ActId,
                            _replayContext.AreaId));
                    break;
                case LayoutLoader.LayoutLoadingMethod.RandomLayout:
                    MessageBusManager.Instance.Publish(
                        new LoadRandomLayoutMessage(
                            _replayContext.ActId,
                            _replayContext.AreaId,
                            _replayContext.GraphId));
                    break;
                default:
                    Replay();
                    break;
            }
        }

        private void RecordSrsResult(RecordSrsResultMessage message)
        {
            Bootstrap.Instance.SrsService.RecordPractice(
                SrsService.GetSrsEntryKey(
                    _replayContext.ActId,
                    _replayContext.AreaId,
                    _replayContext.GraphId,
                    _replayContext.LayoutId),
                message.Result,
                TimerUiController.timer.Time);
        }

        private void Replay()
        {
            _fogOfWar.Build(_layoutDisplay.texture.width, _layoutDisplay.texture.height);
            _playerController.Initialize();
            MessageBusManager.Instance.Publish(new RestartTimerMessage());
        }
    }
}