using UnityEngine;
using UnityEngine.AddressableAssets;

namespace fireMCG.PathOfLayouts.Campaign
{
    [CreateAssetMenu(menuName = "Path of Layouts/Campaign/Graph", fileName = "Graph_")]
    public sealed class GraphDef : DefBase
    {
        public TagSet tagSet = new TagSet();

        public AssetReferenceT<Texture2D> render;

        public LayoutDef[] layouts;
    }
}