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

        [JsonProperty("bestTimeSeconds")]
        public float bestTimeSeconds;

        [JsonProperty("averageTimeSeconds")]
        public float averageTimeSeconds;

        public SrsLayoutData(string actId, string areaId, string graphId, string layoutId)
        {
            this.actId = actId;
            this.areaId = areaId;
            this.graphId = graphId;
            this.layoutId = layoutId;
            isLearning = false;
            masteryLevel = 0;
            timesPracticed = 0;
            timesSucceeded = 0;
            timesFailed = 0;
            lastPracticedUtc = null;
            lastResult = null;
            streak = 0;
            bestTimeSeconds = 0;
            averageTimeSeconds = 0;
        }
    }
}