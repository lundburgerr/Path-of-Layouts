using Newtonsoft.Json;
using System;

namespace fireMCG.PathOfLayouts.Srs
{
    [Serializable]
    public sealed class SrsLayoutData
    {
        [JsonProperty("actId")]
        public string actId;

        [JsonProperty("areaId")]
        public string areaId;

        [JsonProperty("graphId")]
        public string graphId;

        [JsonProperty("layoutId")]
        public string layoutId;
    }
}