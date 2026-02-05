using System;

namespace fireMCG.PathOfLayouts.Messaging
{
	internal static class Validation
	{
		internal static Type IsMessageType(Type type)
		{
			if(type == null)
			{
				throw new ArgumentNullException(nameof(type), "Validation.IsMessageType error, message type is null.");
			}
			if(!typeof(IMessage).IsAssignableFrom(type))
			{
				throw new ArgumentException($"Validation.IsMessageType error, message type '{type?.FullName ?? "null"}' is not of type '{typeof(IMessage).FullName}'.");
			}

			return type;
		}
	}
}