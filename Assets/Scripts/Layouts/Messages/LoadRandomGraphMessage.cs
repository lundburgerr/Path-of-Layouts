using fireMCG.PathOfLayouts.Messaging;

namespace fireMCG.PathOfLayouts.Layouts
{
    public sealed class LoadRandomGraphMessage : IMessage
    {
        public readonly string AreaId;

        public LoadRandomGraphMessage(string areaId)
        {
            AreaId = areaId;
        }
    }
}