using fireMCG.PathOfLayouts.Core;
using fireMCG.PathOfLayouts.Messaging;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private RectTransform _cameraTransform;
        [SerializeField] private RectTransform _playerVisualTransform;
        [SerializeField] private CollisionMap _collisionMap;
        [SerializeField] private FogOfWar _fogOfWar;

        [SerializeField] private int _playerVisualRadius = 4;
        [SerializeField] private int _pixelSpeedPerSecond = 40;

        private int _lightRadiusPercent = 0;

        private int _sprintingPercent = 50;
        private int _movementSpeedPercent = 0;

        private bool _isSprinting = false;

        public Vector2Int PlayerPixelPosition
        {
            get
            {
                return new Vector2Int((int)_playerPosition.x, (int)_playerPosition.y);
            }
        }

        private bool _isReady = false;
        private bool _hasStarted = false;
        private Vector2 _playerPosition;

        private void Awake()
        {
            Assert.IsNotNull(_collisionMap);
            Assert.IsNotNull(_cameraTransform);
            Assert.IsNotNull(_playerVisualTransform);
            Assert.IsNotNull(_fogOfWar);

            _movementSpeedPercent = PlayerPrefs.GetInt("movementSpeed");
            _lightRadiusPercent = PlayerPrefs.GetInt("lightRadius");

            Clear();
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
            MessageBusManager.Instance.Subscribe<OnGameplaySettingsChangedMessage>(OnGameplaySettingsChanged);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Instance.Unsubscribe<OnGameplaySettingsChangedMessage>(OnGameplaySettingsChanged);
        }

        private void OnGameplaySettingsChanged(OnGameplaySettingsChangedMessage message)
        {
            _movementSpeedPercent = message.MovementSpeedPercent;
            _lightRadiusPercent = message.LightRadiusPercent;
        }

        private void Update()
        {
            if (StateController.CurrentState != StateController.AppState.Gameplay)
            {
                return;
            }

            if (!_isReady)
            {
                return;
            }

            var data = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            if (Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 mousePixelPosition = Input.mousePosition;
                Vector2 screenCenter = new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2);

                Vector2 displacementVector = mousePixelPosition - screenCenter;
                _playerPosition += displacementVector;
                _cameraTransform.anchoredPosition = -PlayerPixelPosition;
                _fogOfWar.RevealAt(PlayerPixelPosition, _lightRadiusPercent);
            }
        }

        private void FixedUpdate()
        {
            if (StateController.CurrentState != StateController.AppState.Gameplay)
            {
                return;
            }

            if (!_isReady)
            {
                return;
            }

            if (!_collisionMap.IsBuilt)
            {
                Debug.LogError("PlayerController.FixedUpdate error, collision is not built.");

                return;
            }

            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            _isSprinting = Input.GetKey(KeyCode.Space);

            float movementSpeedModifier = 1 + ((_isSprinting ? _sprintingPercent : 0) + _movementSpeedPercent) / 100f;
            float movementSpeed = _pixelSpeedPerSecond * movementSpeedModifier;

            Vector2 moveDirection = new Vector2(x, y).normalized;
            _playerPosition += movementSpeed * Time.fixedDeltaTime * moveDirection;
            _cameraTransform.anchoredPosition = -PlayerPixelPosition;

            _fogOfWar.RevealAt(PlayerPixelPosition, _lightRadiusPercent);

            if(!_hasStarted && moveDirection.sqrMagnitude != 0)
            {
                MessageBusManager.Instance.Publish(new StartTimerMessage());
                _hasStarted = true;
            }
        }

        public void Initialize()
        {
            Clear();

            _playerVisualTransform.sizeDelta = new Vector2(_playerVisualRadius * 2, _playerVisualRadius * 2);

            _playerPosition = _collisionMap.GetSpawnPoint();
            _fogOfWar.RevealAt(PlayerPixelPosition, _lightRadiusPercent);

            _isReady = true;
        }

        public void Clear()
        {
            _isReady = false;
            _hasStarted = false;
            _cameraTransform.anchoredPosition = Vector2.zero;
            _playerPosition = Vector2.zero;
        }
    }
}