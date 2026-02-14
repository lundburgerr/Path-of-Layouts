using fireMCG.PathOfLayouts.Common;
using System;

namespace fireMCG.PathOfLayouts.Srs
{
    public static class SrsLayoutDataExtension
    {
        public static float GetSuccessRate(this SrsLayoutData data)
        {
            return 100f / (data.timesSucceeded + data.timesFailed) * data.timesSucceeded;
        }

        public static DateTime GetDueDateTime(this SrsLayoutData data)
        {
            if(!DateTimeExtension.TryParseIsoUtc(data.lastPracticedUtc, out DateTime lastPracticed))
            {
                lastPracticed = DateTime.MinValue;
            }

            return lastPracticed.Add(SrsScheduler.MasteryIntervals[data.masteryLevel]);
        }

        public static float GetRunningAverageTime(this SrsLayoutData data, float newTimeSeconds)
        {
            if (data.timesPracticed == 0)
            {
                return newTimeSeconds;
            }

            return ((data.averageTimeSeconds * data.timesPracticed) + newTimeSeconds) / (data.timesPracticed + 1);
        }
    }
}