using fireMCG.PathOfLayouts.Messaging;

namespace fireMCG.PathOfLayouts.IO
{
    public class RegisterPersistableMessage : IMessage
    {
        public readonly IPersistable Persistable;

        public RegisterPersistableMessage(IPersistable persistable)
        {
            Persistable = persistable;
        }
    }
}