using fireMCG.PathOfLayouts.Messaging;

namespace fireMCG.PathOfLayouts.Layouts
{
    public sealed class LoadTargetLayoutMessage : IMessage
    {
        public readonly string LayoutId;

        public LoadTargetLayoutMessage(string layoutId)
        {
            LayoutId = layoutId;
        }
    }
}