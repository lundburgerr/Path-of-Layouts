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
            RegisterMessageListeners();

            _cancelTokenSource = new();
        }

        private void OnDestroy()
        {
            UnregisterMessageListeners();

            _cancelTokenSource.Cancel();
            _cancelTokenSource?.Dispose();
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
            UnregisterMessageListeners();

            MessageBusManager.Resolve.Subscribe<RegisterPersistableMessage>(OnRegisterPersistable);
            MessageBusManager.Resolve.Subscribe<OnPersistableSetDirtyMessage>(OnPersistableSetDirty);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Resolve.Unsubscribe<RegisterPersistableMessage>(OnRegisterPersistable);
            MessageBusManager.Resolve.Unsubscribe<OnPersistableSetDirtyMessage>(OnPersistableSetDirty);
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