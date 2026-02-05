using Newtonsoft.Json;
using System;

namespace fireMCG.PathOfLayouts.LayoutBrowser
{
    [Serializable]
    public sealed class BrowserLayoutData
    {
        [JsonProperty("layoutId")]
        public string layoutId;

        [JsonProperty("displayName")]
        public string displayName;

        [JsonProperty("flags")]
        public string[] flags;
    }
}