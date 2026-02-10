using fireMCG.PathOfLayouts.Messaging;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public class OnReplayLayoutMessage : IMessage
    {
        public readonly bool IsRandom;

        public OnReplayLayoutMessage(bool isRandom)
        {
            IsRandom = isRandom;
        }
    }
}