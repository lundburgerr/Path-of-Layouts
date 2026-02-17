using UnityEngine;

namespace fireMCG.PathOfLayouts.Campaign
{
    [CreateAssetMenu(menuName = "Path of Layouts/Campaign/Area", fileName = "Area_")]
    public sealed class AreaDef : DefBase
    {
        public Texture2D thumbnail;

        public GraphDef[] graphs;
    }
}