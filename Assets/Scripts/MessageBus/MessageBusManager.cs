using System;
using System.Collections.Generic;

namespace fireMCG.PathOfLayouts.Messaging
{
	public sealed class MessageBusManager : IMessageBus
	{
		private static readonly MessageBusManager _instance;

		private readonly Dictionary<Type, ListenersForMessageType> _listeners = new Dictionary<Type, ListenersForMessageType>();

		static MessageBusManager()
		{
			_instance = new MessageBusManager();
		}

		public static IMessageBus Resolve => _instance;

		public void Subscribe<T>(MessageListener<T> listener) where T : IMessage
		{
			ListenersForMessageType listenersForMessageType = GetListeners(typeof(T));
			listenersForMessageType.Add(listener);
		}

		public void Unsubscribe<T>(MessageListener<T> listener) where T : IMessage
		{
			ListenersForMessageType listenersForMessageType = GetListeners(typeof(T));

			listenersForMessageType.Remove(listener);
		}

		public void Publish(IMessage message)
		{
			Type publishToMessageType = Validation.IsMessageType(message?.GetType());
			while(publishToMessageType != null)
			{
				ListenersForMessageType listeners = GetListeners(publishToMessageType);
				listeners.Publish(message);
				publishToMessageType = listeners.MessageBaseType;
			}
		}

		private ListenersForMessageType GetListeners(Type messageType)
		{
			Validation.IsMessageType(messageType);

			if(!_listeners.TryGetValue(messageType, out ListenersForMessageType listenersForMessageType))
			{
				listenersForMessageType = _listeners[messageType] = new ListenersForMessageType(messageType);
			}

			return listenersForMessageType;
		}
	}
}