using fireMCG.PathOfLayouts.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace fireMCG.PathOfLayouts.Manifest
{
    public static class CampaignManifestExtension
    {
        public static IReadOnlyList<AreaEntry> GetAreas(this CampaignManifest manifest, string actId)
        {
            return manifest.acts
                .FirstOrDefault(act => act.actId == actId).areas;
        }

        public static IReadOnlyList<GraphEntry> GetGraphs(this CampaignManifest manifest, string actId, string areaId)
        {
            return manifest.acts
                .FirstOrDefault(act => act.actId == actId).areas
                .FirstOrDefault(area =>  area.areaId == areaId).graphs;
        }

        public static IReadOnlyList<string> GetLayoutIds(this CampaignManifest manifest, string actId, string areaId, string graphId)
        {
            string path = StreamingPathResolver.GetGraphFolderPath(actId, areaId, graphId);

            if (!Directory.Exists(path))
            {
                return Array.Empty<string>();
            }

            string[] suffixes = new[]
            {
                StreamingPathResolver.LAYOUT_SUFFIX,
                StreamingPathResolver.COLLISION_MAP_SUFFIX
            };

            HashSet<string> layoutIds = new();

            foreach(string file in Directory.EnumerateFiles(path))
            {
                string fileName = Path.GetFileName(file);
                
                foreach(string suffix in suffixes)
                {
                    if(fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        fileName = fileName.Replace(suffix, "");
                        layoutIds.Add(fileName);

                        break;
                    }
                }
            }

            return layoutIds.ToList();
        }
    }
}