using fireMCG.PathOfLayouts.Messaging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace fireMCG.PathOfLayouts.IO
{
    public class DataPersistenceManager : MonoBehaviour
    {
        private readonly List<IPersistable> _persistables = new();
        private CancellationTokenSource _cancelTokenSource;

        private bool _saveScheduled = false;
        private float _nextSaveTime = 0f;
        private float _saveInterval = 5f;

        private void Awake()
        {
            _cancelTokenSource = new();
        }

        private void OnEnable()
        {
            RegisterMessageListeners();
        }

        private void OnDisable()
        {
            UnregisterMessageListeners();
        }

        private void OnDestroy()
        {
            if (_cancelTokenSource != null)
            {
                _cancelTokenSource.Cancel();
                _cancelTokenSource?.Dispose();
                _cancelTokenSource = null;
            }
        }

        private void OnApplicationQuit()
        {
            _ = SaveAllAsync();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                return;
            }

            _ = SaveAllAsync();
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                return;
            }

            _ = SaveAllAsync();
        }

        private void Update()
        {
            if (!_saveScheduled)
            {
                return;
            }

            if(Time.unscaledTime >= _nextSaveTime)
            {
                _saveScheduled = false;
                _ = SaveAllAsync();
            }
        }

        private void RegisterMessageListeners()
        {
            MessageBusManager.Instance.Subscribe<RegisterPersistableMessage>(OnRegisterPersistable);
            MessageBusManager.Instance.Subscribe<OnPersistableSetDirtyMessage>(OnPersistableSetDirty);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Instance.Unsubscribe<RegisterPersistableMessage>(OnRegisterPersistable);
            MessageBusManager.Instance.Unsubscribe<OnPersistableSetDirtyMessage>(OnPersistableSetDirty);
        }

        private void OnRegisterPersistable(RegisterPersistableMessage message)
        {
            if (_persistables.Contains(message.Persistable))
            {
                return;
            }

            _persistables.Add(message.Persistable);
        }

        private void OnPersistableSetDirty(OnPersistableSetDirtyMessage message)
        {
            _nextSaveTime = Time.unscaledTime + _saveInterval;
            _saveScheduled = true;
        }

        private async Task SaveAllAsync()
        {
            foreach(IPersistable persistable in _persistables)
            {
                if(persistable is null || !persistable.IsDirty)
                {
                    continue;
                }

                try
                {
                    await persistable.SaveAsync(_cancelTokenSource.Token);
                    Debug.Log($"DataPersistenceManager.SaveAllAsync saved file {persistable.Name}.");
                }
                catch(System.Exception e)
                {
                    Debug.Log($"DataPersistenceManager.SaveAllAsync error, failed to save file {persistable.Name}. e={e}");
                }
            }
        }
    }
}