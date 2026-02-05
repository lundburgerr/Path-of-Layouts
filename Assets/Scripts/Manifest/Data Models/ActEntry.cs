using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace fireMCG.PathOfLayouts.Manifest
{
    [Serializable]
    public sealed class ActEntry
    {
        [JsonProperty("actId")]
        public string actId;

        [JsonProperty("areas")]
        public List<AreaEntry> areas = new();
    }
}