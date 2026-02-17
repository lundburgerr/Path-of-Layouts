using fireMCG.PathOfLayouts.Messaging;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Layouts
{
    public class OnLayoutLoadedMessage : IMessage
    {
        public readonly string RootId;
        public readonly string LayoutId;
        public readonly Texture2D LayoutMap;
        public readonly LayoutLoader.LayoutLoadingMethod LayoutLoadingMethod;

        public OnLayoutLoadedMessage(string rootId, string layoutId, Texture2D layoutMap, LayoutLoader.LayoutLoadingMethod layoutLoadingMethod)
        {
            RootId = rootId;
            LayoutId = layoutId;
            LayoutMap = layoutMap;
            LayoutLoadingMethod = layoutLoadingMethod;
        }
    }
}