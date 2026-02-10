#if UNITY_EDITOR
using fireMCG.PathOfLayouts.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Manifest
{
    public static class CampaignManifestBuilder
    {
        public const int MANIFEST_SCHEMA_VERSION = 1;

        private static readonly string[] GraphExtensions =
        {
            ".dgr.json",
            ".tgr.json"
        };

        [MenuItem("Tools/Path of Layouts/Build Campaign Manifest")]
        public static void BuildAndWriteManifest()
        {
            string manifestRootPath = StreamingPathResolver.GetRootPath();

            if (!Directory.Exists(manifestRootPath))
            {
                Debug.LogError($"CampaignManifestBuilder.BuildAndWriteManifest error, the {manifestRootPath} folder does not exist.");

                return;
            }

            CampaignManifest manifest = BuildManifestObject();
            string json = JsonConvert.SerializeObject(manifest, Formatting.Indented);

            File.WriteAllText(StreamingPathResolver.GetManifestFilePath(), json);
        }

        private static CampaignManifest BuildManifestObject()
        {
            CampaignManifest manifest = new CampaignManifest()
            {
                schemaVersion = MANIFEST_SCHEMA_VERSION
            };

            BuildManifestData(manifest);

            return manifest;
        }

        private static void BuildManifestData(CampaignManifest manifest)
        {
            string[] actDirectories = Directory.GetDirectories(StreamingPathResolver.GetRootPath())
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<ActEntry> actEntries = new();
            foreach (string directory in actDirectories)
            {
                string folderName = Path.GetFileName(directory);

                List<AreaEntry> areaEntries = BuildAreas(directory);

                ActEntry entry = new ActEntry()
                {
                    actId = folderName,
                    areas = areaEntries
                };

                actEntries.Add(entry);
            }

            manifest.acts = actEntries;
        }

        private static List<AreaEntry> BuildAreas(string actDirectory)
        {
            string[] areaDirectories = Directory.GetDirectories(actDirectory);

            List<AreaEntry> areaEntries = new();
            foreach(string directory in areaDirectories)
            {
                string folderName = Path.GetFileName(directory);

                List<GraphEntry> graphEntries = BuildGraphs(directory);

                AreaEntry entry = new()
                {
                    areaId = folderName,
                    graphs = graphEntries
                };

                areaEntries.Add(entry);
            }

            return areaEntries;
        }

        private static List<GraphEntry> BuildGraphs(string areaDirectory)
        {
            string[] graphs = Directory.GetFiles(areaDirectory);

            List<GraphEntry> graphEntries = new();
            foreach (string graph in graphs)
            {
                string graphId = Path.GetFileName(graph);
                string extension = GetFileExtension(graph);

                if (string.IsNullOrEmpty(extension))
                {
                    continue;
                }

                graphId = graphId.Replace(extension, string.Empty);

                GraphEntry entry = new GraphEntry()
                {
                    graphId = graphId,
                    fileExtension = extension,
                };

                graphEntries.Add(entry);
            }

            return graphEntries;
        }

        private static string GetFileExtension(string fileAbsolutePath)
        {
            string fileName = Path.GetFileName(fileAbsolutePath);

            foreach(string extension in GraphExtensions.OrderByDescending(e => e.Length))
            {
                if (fileName.EndsWith(extension))
                {
                    return extension;
                }
            }

            return string.Empty;
        }
    }
}
#endif