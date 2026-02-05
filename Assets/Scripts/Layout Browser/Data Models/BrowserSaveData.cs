using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace fireMCG.PathOfLayouts.LayoutBrowser
{
    [Serializable]
    public sealed class BrowserSaveData
    {
        [JsonProperty("layouts")]
        public List<BrowserLayoutData> layouts = new();
    }
}