using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace fireMCG.PathOfLayouts.Srs
{
    [Serializable]
    public sealed class SrsSaveData
    {
        [JsonProperty("layouts")]
        public List<SrsLayoutData> layouts = new();
    }
}