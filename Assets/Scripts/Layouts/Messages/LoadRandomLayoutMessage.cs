using fireMCG.PathOfLayouts.Messaging;

namespace fireMCG.PathOfLayouts.Layouts
{
    public sealed class LoadRandomLayoutMessage : IMessage
    {
        public readonly string GraphId;

        public LoadRandomLayoutMessage(string graphId)
        {
            GraphId = graphId;
        }
    }
}