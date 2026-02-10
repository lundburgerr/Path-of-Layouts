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
                lastPracticed = DateTime.UtcNow;
            }
            
            return lastPracticed.Add(SrsScheduler.MasteryIntervals[data.masteryLevel]);
        }
    }
}