using System;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Messaging
{
	internal sealed class ListenersForMessageType
	{
		private Delegate _listeners;

		public Type MessageType { get; }

		public Type MessageBaseType { get; }

		public ListenersForMessageType(Type messageType)
		{
			MessageType = Validation.IsMessageType(messageType);

			if(MessageType.BaseType == null || typeof(IMessage).IsAssignableFrom(MessageType.BaseType))
			{
				MessageBaseType = MessageType.BaseType;
			}
			else
			{
				MessageBaseType = typeof(IMessage);
			}
		}

		public void Add(Delegate listener)
		{
			if(listener == null)
			{
				throw new ArgumentNullException(nameof(listener));
			}

			foreach(Delegate existingListener in _listeners?.GetInvocationList() ?? Array.Empty<Delegate>())
			{
				if(existingListener == listener)
				{
					throw new ArgumentException($"Listener is already registered.", nameof(listener));
				}
			}
			_listeners = Delegate.Combine(_listeners, listener);
		}

		public bool Remove(Delegate listener)
		{
			int listenerCount = _listeners?.GetInvocationList().Length ?? 0;
			_listeners = Delegate.Remove(_listeners, listener);

			return (_listeners?.GetInvocationList().Length ?? 0) < listenerCount;
		}

		public void Publish(IMessage message)
		{
			foreach(Delegate listener in _listeners?.GetInvocationList() ?? Array.Empty<Delegate>())
			{
				try
				{
					listener.DynamicInvoke(message);
				}
				catch(Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		public override string ToString()
		{
			int listenerCount = _listeners?.GetInvocationList().Length ?? 0;
			string listenersText = listenerCount == 1 ? "listener" : "listeners";

			return $"Listeners[{MessageType.Name}] ({listenerCount} {listenersText})";
		}
	}
}