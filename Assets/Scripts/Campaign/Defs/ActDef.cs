using UnityEngine;

namespace fireMCG.PathOfLayouts.Campaign
{
    [CreateAssetMenu(menuName = "Path of Layouts/Campaign/Act", fileName = "Act_")]
    public sealed class ActDef : DefBase
    {
        public AreaDef[] areas;
    }
}