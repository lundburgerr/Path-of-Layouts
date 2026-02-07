using fireMCG.PathOfLayouts.Core;
using NUnit.Framework;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private CollisionMap _collisionMap;
        [SerializeField] private RectTransform _cameraTransform;
        [SerializeField] private RectTransform _playerVisualTransform;
        [SerializeField] private FogOfWar _fogOfWar;

        [SerializeField] private int _playerVisualRadius = 4;
        [SerializeField] private int _pixelSpeedPerSecond = 60;

        public Vector2Int PlayerPixelPosition
        {
            get
            {
                return new Vector2Int((int)_playerPosition.x, (int)_playerPosition.y);
            }
        }

        private bool _isReady = false;
        private Vector2 _playerPosition;

        private void Awake()
        {
            Assert.IsNotNull(_collisionMap);
            Assert.IsNotNull(_cameraTransform);
            Assert.IsNotNull(_playerVisualTransform);
            Assert.IsNotNull(_fogOfWar);
        }

        private void OnDestroy()
        {
            Clear();
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

            Vector2 moveDirection = new Vector2(x, y).normalized;
            _playerPosition += _pixelSpeedPerSecond * Time.fixedDeltaTime * moveDirection;
            _cameraTransform.anchoredPosition = -PlayerPixelPosition;

            _fogOfWar.RevealAt(PlayerPixelPosition);
        }

        public void Initialize()
        {
            Clear();

            _playerVisualTransform.sizeDelta = new Vector2(_playerVisualRadius * 2, _playerVisualRadius * 2);

            _playerPosition = _collisionMap.GetSpawnPoint();
            _fogOfWar.RevealAt(PlayerPixelPosition);

            _isReady = true;
        }

        public void Clear()
        {
            _isReady = false;
            _cameraTransform.anchoredPosition = Vector2.zero;
            _playerPosition = Vector2.zero;
        }
    }
}