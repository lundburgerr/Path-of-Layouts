using fireMCG.PathOfLayouts.Common;
using fireMCG.PathOfLayouts.IO;
using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.Prompt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Srs
{
    public class SrsService
    {
        public SrsSaveData SrsData { get; private set; }

        public async Task LoadSrsSaveDataAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            SrsData = await JsonFileStore.LoadOrCreateAsync(
                PersistentPathResolver.GetSrsFilePath(),
                SrsSaveData.CreateDefault,
                token);

            if (SrsData is null)
            {
                throw new InvalidOperationException($"SrsService.LoadSrsSaveDataAsync error, Srs load returned null data");
            }
        }

        public async Task SaveSrsDataAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if(SrsData is null)
            {
                throw new InvalidOperationException("SrsService.SaveSrsDataAsync error, Srs data is null");
            }

            await JsonFileStore.SaveAsync(
                PersistentPathResolver.GetSrsFilePath(),
                SrsData,
                token);
        }

        public void SetDefaultData()
        {
            SrsData = SrsSaveData.CreateDefault();
        }

        public void AddToLearning(string srsEntryKey)
        {
            if (!TryValidateKey(srsEntryKey, "SrsService.AddToLearning", "Error adding srs entry to the learning queue"))
            {
                return;
            }

            SrsLayoutData layoutData;

            if (SrsData.layouts.TryGetValue(srsEntryKey, out SrsLayoutData data))
            {
                data.isLearning = true;

                // To do: Srs Layout Added/Enabled Message(s)
            }
            else
            {
                layoutData = new()
                {
                    isLearning = true
                };

                SrsData.layouts.Add(srsEntryKey, layoutData);

                // To do: Srs Layout Added/Enabled Message(s)
            }
        }

        public void RemoveFromLearning(string srsEntryKey)
        {
            if (!TryValidateKey(srsEntryKey, "SrsService.RemoveFromLearning", "Error removing srs entry from the learning queue"))
            {
                return;
            }

            if (!TryGetKeyValue(srsEntryKey, "RemoveFromLearning", "Error removing srs entry from the learning queue", out SrsLayoutData data))
            {
                return;
            }

            data.isLearning = false;

            // To do: Srs Layout Removed/Disabled Message(s)
        }

        public void RecordPractice(string srsEntryKey, SrsPracticeResult result)
        {
            if (!TryValidateKey(srsEntryKey, "SrsService.RecordPractice", "Error recording practice results"))
            {
                return;
            }

            if(!TryGetKeyValue(srsEntryKey, "RecordPractice", "Error recording practice results", out SrsLayoutData data))
            {
                return;
            }

            data.masteryLevel = SrsScheduler.ClampMastery(data.masteryLevel + (result == SrsPracticeResult.Success ? 1 : -1));
            
            data.timesPracticed++;
            data.timesSucceeded += result == SrsPracticeResult.Success ? 1 : 0;
            data.timesFailed += result == SrsPracticeResult.Failure ? 1 : 0;
            
            if(data.lastResult == result.ToString())
            {
                data.streak++;
            }
            else
            {
                data.streak = 1;
            }
            data.lastResult = result.ToString();

            data.lastPracticedUtc = DateTime.UtcNow.ToIsoUtc();

            // To do: Srs Layout Practice Recorded Message
        }

        public IReadOnlyList<SrsLayoutData> GetDueLayouts(DateTime? nowUtc = null, int? limit = null)
        {
            DateTime now = nowUtc ?? DateTime.UtcNow;

            IEnumerable<SrsLayoutData> query = SrsData.layouts.Values
                .Where(l => l is not null && l.isLearning)
                .Select(l => (Layout: l, Due: l.GetDueDateTime()))
                .Where(t => now >= t.Due)
                .OrderBy(t => t.Due)
                .ThenBy(t => t.Layout.masteryLevel)
                .Select(t => t.Layout);

            return ApplyLimit(query, limit).ToList();
        }

        public IReadOnlyList<SrsLayoutData> GetNextDueLayouts(int limit) => GetNextDueLayouts(null, limit);

        public IReadOnlyList<SrsLayoutData> GetNextDueLayouts(DateTime? nowUtc = null, int? limit = null)
        {
            DateTime now = nowUtc ?? DateTime.UtcNow;

            IEnumerable<SrsLayoutData> query = SrsData.layouts.Values
                .Where(l => l is not null && l.isLearning)
                .Select(l => (Layout: l, Due: l.GetDueDateTime()))
                .Where(t => now < t.Due)
                .OrderBy(t => t.Due)
                .ThenBy(t => t.Layout.masteryLevel)
                .Select(t => t.Layout);

            return ApplyLimit(query, limit).ToList();
        }

        public IReadOnlyList<SrsLayoutData> GetBurntLayouts(int? limit = null)
        {
            IEnumerable<SrsLayoutData> query = SrsData.layouts.Values
                .Where(l => l.masteryLevel == SrsScheduler.MasteryIntervals.Length - 1)
                .OrderByDescending(l => l.lastPracticedUtc);

            return ApplyLimit(query, limit).ToList();
        }

        public IReadOnlyList<SrsLayoutData> GetDisabledLayouts(int? limit = null)
        {
            IEnumerable<SrsLayoutData> query = SrsData.layouts.Values
                .Where(l => !l.isLearning)
                .OrderBy(l => l.masteryLevel)
                .ThenBy(l => l.lastPracticedUtc);

            return ApplyLimit(query, limit).ToList();
        }

        public int GetLayoutsDueWithin(TimeSpan timeSpan)
        {
            DateTime dueWithinData = DateTime.UtcNow.Add(timeSpan);

            return SrsData.layouts.Values
                .Where(l => l.isLearning && l.GetDueDateTime() < dueWithinData)
                .Count();
        }

        public bool IsLayoutDue(SrsLayoutData layoutData, DateTime? nowUtc = null)
        {
            if (layoutData is null || !layoutData.isLearning)
            {
                return false;
            }

            DateTime now = nowUtc ?? DateTime.UtcNow;

            return now >= layoutData.GetDueDateTime();
        }

        // Includes all ids since layoutIds are file names attributed by users which aren't forced to be unique
        public static string GetSrsEntryKey(string actId, string areaId, string graphId, string layoutId)
        {
            return $"{actId}-{areaId}-{graphId}-{layoutId}";
        }

        private static IEnumerable<T> ApplyLimit<T>(IEnumerable<T> query, int? limit)
        {
            return (limit is > 0) ? query.Take(limit.Value) : query;
        }

        private bool TryValidateKey(string key, string methodName, string userFacingHeader)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            string details =
                $"{userFacingHeader}\n" +
                $"Srs entry key is invalid" +
                $"key={key}";

            LogAndPublishError(methodName, "key is invalid", key, details);

            return true;
        }

        private bool TryGetKeyValue(string key, string methodName, string userFacingHeader, out SrsLayoutData srsLayoutData)
        {
            srsLayoutData = null;

            if(SrsData is null)
            {
                string details =
                    $"{userFacingHeader}\n" +
                    $"Srs data is null\n" +
                    $"key={key}";

                LogAndPublishError(methodName, "Srs data is null", key, details);

                return false;
            }

            if (!SrsData.layouts.TryGetValue(key, out srsLayoutData))
            {
                string details =
                    $"{userFacingHeader}\n" +
                    "Srs entry key can't be found\n" +
                    $"key={key}";

                LogAndPublishError(methodName, "key can't be found", key, details);

                return false;
            }

            return true;
        }

        private static void LogAndPublishError(string methodName, string error, string srsEntryKey, string errorMessage)
        {
            Debug.LogError($"{methodName} error, {error}. key={srsEntryKey}");
            MessageBusManager.Resolve.Publish(new OnErrorMessage(errorMessage));
        }
    }
}