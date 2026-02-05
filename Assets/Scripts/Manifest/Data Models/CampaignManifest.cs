using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace fireMCG.PathOfLayouts.Manifest
{
    [Serializable]
    public sealed class CampaignManifest
    {
        [JsonProperty("schemaVersion")]
        public int schemaVersion = 1;

        [JsonProperty("acts")]
        public List<ActEntry> acts = new();
    }
}