using fireMCG.PathOfLayouts.Common;
using fireMCG.PathOfLayouts.IO;
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
                throw new InvalidOperationException($"SrsService.LoadSrsSaveDataAsync error, Srs load returned null data.");
            }
        }

        public async Task SaveSrsDataAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if(SrsData is null)
            {
                throw new InvalidOperationException("SrsService.SaveSrsDataAsync error, Srs data is null.");
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

        public void AddToLearning(string srsLayoutKey)
        {
            if(string.IsNullOrWhiteSpace(srsLayoutKey))
            {
                Debug.LogError($"SrsService.AddToLearning error, key is invalid. key={srsLayoutKey}");

                // To do: Ui Error Prompt Message

                return;
            }

            SrsLayoutData layoutData;

            if (SrsData.layouts.TryGetValue(srsLayoutKey, out SrsLayoutData data))
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

                SrsData.layouts.Add(srsLayoutKey, layoutData);

                // To do: Srs Layout Added/Enabled Message(s)
            }
        }

        public void RemoveFromLearning(string srsLayoutKey)
        {
            if (string.IsNullOrWhiteSpace(srsLayoutKey))
            {
                Debug.LogError($"SrsService.RemoveFromLearning error, key is invalid. key={srsLayoutKey}");

                // To do: Ui Error Prompt Message

                return;
            }

            if (!SrsData.layouts.TryGetValue(srsLayoutKey, out SrsLayoutData data))
            {
                Debug.LogError($"SrsService.RemoveFromLearning error, key can't be found. key={srsLayoutKey}");

                // To do: Ui Error Prompt Message

                return;
            }

            data.isLearning = false;

            // To do: Srs Layout Removed/Disabled Message(s)
        }

        public void RecordPractice(string srsLayoutKey, SrsPracticeResult result)
        {
            if (string.IsNullOrWhiteSpace(srsLayoutKey))
            {
                Debug.LogError($"SrsService.RecordPractice error, key is invalid. key={srsLayoutKey}");

                // To do: Ui Error Prompt Message

                return;
            }

            if (!SrsData.layouts.TryGetValue(srsLayoutKey, out SrsLayoutData data))
            {
                Debug.LogError($"SrsService.RecordPractice error, key can't be found. key={srsLayoutKey}");

                // To do: Ui Error Prompt Message

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

        public IReadOnlyList<SrsLayoutData> GetDueLayouts(DateTime? nowUtc = null)
        {
            DateTime now = nowUtc ?? DateTime.UtcNow;

            return SrsData.layouts.Values
                .Where(l => l is not null && l.isLearning)
                .Select(l => (Layout: l, Due: l.GetDueDateTime()))
                .Where(t => now >= t.Due)
                .OrderBy(t => t.Due)
                .ThenBy(t => t.Layout.masteryLevel)
                .Select(t => t.Layout)
                .ToList();
        }

        public IReadOnlyList<SrsLayoutData> GetNextDueLayouts(DateTime? nowUtc = null)
        {
            DateTime now = nowUtc ?? DateTime.UtcNow;

            return SrsData.layouts.Values
                .Where(l => l is not null && l.isLearning)
                .Select(l => (Layout: l, Due: l.GetDueDateTime()))
                .Where(t => now < t.Due)
                .OrderBy(t => t.Due)
                .ThenBy(t => t.Layout.masteryLevel)
                .Select(t => t.Layout)
                .ToList();
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
        public static string GetSrsLayoutKey(string actId, string areaId, string graphId, string layoutId)
        {
            return $"{actId}-{areaId}-{graphId}-{layoutId}";
        }
    }
}