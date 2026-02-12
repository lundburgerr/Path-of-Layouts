using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.Srs;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public class RecordSrsResultMessage : IMessage
    {
        public readonly SrsPracticeResult Result;

        public RecordSrsResultMessage(SrsPracticeResult result)
        {
            Result = result;
        }
    }
}