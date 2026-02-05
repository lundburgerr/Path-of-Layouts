namespace fireMCG.PathOfLayouts.Messaging
{
    public sealed class PlayTargetLayoutMessage : IMessage
    {
        public readonly string actId;
        public readonly string areaId;
        public readonly string graphId;
        public readonly string layoutId;

        public PlayTargetLayoutMessage(string actId, string areaId, string graphId, string layoutId)
        {
            this.actId = actId;
            this.areaId = areaId;
            this.graphId = graphId;
            this.layoutId = layoutId;
        }
    }
}