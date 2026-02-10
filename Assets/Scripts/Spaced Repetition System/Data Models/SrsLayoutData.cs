using Newtonsoft.Json;
using System;

namespace fireMCG.PathOfLayouts.Srs
{
    [Serializable]
    public sealed class SrsLayoutData
    {
        [JsonProperty("isLearning")]
        public bool isLearning;

        [JsonProperty("masteryLevel")]
        public int masteryLevel;

        [JsonProperty("timesPracticed")]
        public int timesPracticed;

        [JsonProperty("timesSucceeded")]
        public int timesSucceeded;

        [JsonProperty("timesFailed")]
        public int timesFailed;

        [JsonProperty("lastPracticedUtc")]
        public string lastPracticedUtc;

        [JsonProperty("lastResult")]
        public string lastResult;

        [JsonProperty("streak")]
        public int streak;

        public SrsLayoutData()
        {
            isLearning = false;
            masteryLevel = 0;
            timesPracticed = 0;
            timesSucceeded = 0;
            timesFailed = 0;
            lastPracticedUtc = null;
            lastResult = null;
            streak = 0;
        }
    }
}