#if UNITY_EDITOR
using fireMCG.PathOfLayouts.Campaign;
using fireMCG.PathOfLayouts.Content;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace fireMCG.PathOfLayouts.EditorTools
{
    public static class CampaignAddressablesLabelTool
    {
        private const string MENU_APPLY = "Tools/Path of Layouts/Campaign/Apply Addressables Labels (Selected DB)";

        private const string GROUP_GRAPH_RENDERS = "Graph_Renders";
        private const string GROUP_LAYOUT_IMAGES = "Layout_Images";

        [MenuItem(MENU_APPLY)]
        public static void ApplyLabelsToSelectedDatabase()
        {
            CampaignDatabase database = Selection.activeObject as CampaignDatabase;
            if (database == null)
            {
                EditorUtility.DisplayDialog(
                    "Apply Addressables Labels",
                    "Select a CampaignDatabase asset in the Project window first.",
                    "OK");

                return;
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog(
                    "Apply Addressables Labels",
                    "Addressables Settings not found. Open Window > Asset Management > Addressables > Groups to initialize.",
                    "OK");

                return;
            }

            try
            {
                Apply(database, settings);

                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog("Apply Addressables Labels", "Done. Check Console for summary.", "OK");
            }
            catch (Exception e)
            {
                Debug.LogError("Apply Addressables Labels failed: " + e);

                EditorUtility.DisplayDialog("Apply Addressables Labels", "Failed. Check Console.", "OK");

                return;
            }
        }

        private static void Apply(CampaignDatabase database, AddressableAssetSettings settings)
        {
            if (database.acts == null || database.acts.Length == 0)
            {
                Debug.LogError("CampaignDatabase.acts is empty. Assign root acts used for browsing first.");

                return;
            }

            AddressableAssetGroup graphRendersGroup = GetOrCreateGroup(settings, GROUP_GRAPH_RENDERS);
            AddressableAssetGroup layoutImagesGroup = GetOrCreateGroup(settings, GROUP_LAYOUT_IMAGES);

            int entriesCreated = 0;
            int entriesMoved = 0;
            int labelsAdded = 0;

            HashSet<string> processedGuids = new HashSet<string>(StringComparer.Ordinal);

            foreach (ActDef act in database.acts)
            {
                if (act == null)
                {
                    continue;
                }

                if (act.areas == null)
                {
                    continue;
                }

                foreach (AreaDef area in act.areas)
                {
                    if (area == null)
                    {
                        continue;
                    }

                    string areaLabel = AddressablesLabels.AREA_GRAPH_RENDERS_PREFIX + area.id;

                    if (area.graphs == null)
                    {
                        continue;
                    }

                    foreach (GraphDef graph in area.graphs)
                    {
                        if (graph == null)
                        {
                            continue;
                        }

                        // 1) Graph render: label per AREA
                        string renderGuid;
                        bool hasRenderGuid = TryGetGuid(graph.render, out renderGuid);
                        if (hasRenderGuid)
                        {
                            if (processedGuids.Add(renderGuid))
                            {
                                AddressableAssetEntry renderEntry =
                                    EnsureEntry(settings, renderGuid, graphRendersGroup, ref entriesCreated, ref entriesMoved);

                                labelsAdded += EnsureLabel(settings, renderEntry, areaLabel);
                            }
                        }

                        // 2) Layout images: label per GRAPH
                        if (graph.layouts == null)
                        {
                            continue;
                        }

                        string graphLayoutsLabel = AddressablesLabels.GRAPH_LAYOUT_IMAGES_PREFIX + graph.id;

                        foreach (LayoutDef layout in graph.layouts)
                        {
                            if (layout == null)
                            {
                                continue;
                            }

                            string layoutImageGuid;
                            bool hasLayoutImageGuid = TryGetGuid(layout.layoutImage, out layoutImageGuid);
                            if (hasLayoutImageGuid)
                            {
                                if (processedGuids.Add(layoutImageGuid))
                                {
                                    AddressableAssetEntry imageEntry =
                                        EnsureEntry(settings, layoutImageGuid, layoutImagesGroup, ref entriesCreated, ref entriesMoved);

                                    labelsAdded += EnsureLabel(settings, imageEntry, graphLayoutsLabel);
                                }
                            }
                        }
                    }
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);

            Debug.Log(
                "[CampaignAddressablesLabelTool] Completed.\n" +
                "- Entries created: " + entriesCreated + "\n" +
                "- Entries moved to groups: " + entriesMoved + "\n" +
                "- Label adds: " + labelsAdded + "\n" +
                "Labels:\n" +
                "- Graph renders: " + AddressablesLabels.AREA_GRAPH_RENDERS_PREFIX + "<AreaId>\n" +
                "- Layout images: " + AddressablesLabels.GRAPH_LAYOUT_IMAGES_PREFIX + "<GraphId>");
        }

        private static bool TryGetGuid(AssetReference reference, out string guid)
        {
            guid = null;

            if (reference == null)
            {
                return false;
            }

            guid = reference.AssetGUID;

            if (string.IsNullOrWhiteSpace(guid))
            {
                return false;
            }

            return true;
        }

        private static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string groupName)
        {
            AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group != null)
            {
                return group;
            }

            group = settings.CreateGroup(groupName, false, false, true, null);

            return group;
        }

        private static AddressableAssetEntry EnsureEntry(
            AddressableAssetSettings settings,
            string guid,
            AddressableAssetGroup targetGroup,
            ref int created,
            ref int moved)
        {
            AddressableAssetEntry entry = settings.FindAssetEntry(guid);

            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, targetGroup, false, false);

                created++;

                return entry;
            }

            if (entry.parentGroup != targetGroup)
            {
                settings.MoveEntry(entry, targetGroup, false);

                moved++;
            }

            return entry;
        }

        private static int EnsureLabel(AddressableAssetSettings settings, AddressableAssetEntry entry, string label)
        {
            if (entry == null)
            {
                return 0;
            }

            IList<string> labels = settings.GetLabels();

            bool exists = false;
            foreach (string existing in labels)
            {
                if (string.Equals(existing, label, StringComparison.Ordinal))
                {
                    exists = true;

                    break;
                }
            }

            if (!exists)
            {
                settings.AddLabel(label, false);
            }

            if (!entry.labels.Contains(label))
            {
                entry.SetLabel(label, true, false);

                return 1;
            }

            return 0;
        }
    }
}
#endif