#if UNITY_EDITOR
using fireMCG.PathOfLayouts.Campaign;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace fireMCG.PathOfLayouts.EditorTools
{
    public static class CampaignDatabaseRebuildTool
    {
        private const string MENU_REBUILD = "Tools/Path of Layouts/Campaign/Rebuild Campaign Database";
        private const string MENU_VALIDATE = "Tools/Path of Layouts/Campaign/Validate Campaign Database";

        [MenuItem(MENU_REBUILD)]
        public static void RebuildSelectedDatabase()
        {
            CampaignDatabase database = Selection.activeObject as CampaignDatabase;

            if(database is null)
            {
                EditorUtility.DisplayDialog(
                    "Rebuild Campaign Database",
                    "Select a CampaignDatabase asset in the Project window first.",
                    "OK");

                return;
            }

            Rebuild(database, false);
        }

        [MenuItem(MENU_VALIDATE)]
        public static void ValidateSelectionDatabase()
        {
            CampaignDatabase database = Selection.activeObject as CampaignDatabase;

            if(database is null)
            {
                EditorUtility.DisplayDialog(
                    "Validate Campaign Database",
                    "Select a CampaignDatabase asset in the Project window first.",
                    "OK");

                return;
            }

            Validate(database, false);
        }

        public static void Rebuild(CampaignDatabase database, bool strictSlugUniqueness)
        {
            try
            {
                ActDef[] acts = FindAllAssetsOfType<ActDef>();
                AreaDef[] areas = FindAllAssetsOfType<AreaDef>();
                GraphDef[] graphs = FindAllAssetsOfType<GraphDef>();
                LayoutDef[] layouts = FindAllAssetsOfType<LayoutDef>();
                NavigationDataAsset[] navDataAssets = FindAllAssetsOfType<NavigationDataAsset>();

                List<string> errors = new List<string>();
                ValidateUniqueIds("ActDef", acts, errors);
                ValidateUniqueIds("AreaDef", areas, errors);
                ValidateUniqueIds("GraphDef", graphs, errors);
                ValidateUniqueIds("LayoutDef", layouts, errors);

                ValidateHierarchy(database, errors);

                if(errors.Count > 0)
                {
                    Debug.LogError($"CampaignDatabase rebuild failed with {errors.Count} issue(s):\n" + string.Join("\n", errors));
                    EditorUtility.DisplayDialog("Rebuild Failed", $"Found {errors.Count} issue(s). Check Console.", "OK");

                    return;
                }

                Undo.RecordObject(database, "Rebuild Campaign Database");

                database.allAreas = areas;
                database.allGraphs = graphs;
                database.allLayouts = layouts;
                database.allNavigationData = navDataAssets;

                EditorUtility.SetDirty(database);

                MarkDirty(database.acts);
                MarkDirty(database.allAreas);
                MarkDirty(database.allGraphs);

                database.BuildRuntimeIndex();

                AssetDatabase.SaveAssets();

                Debug.Log(
                    "CampaignDatabase rebuilt successfully.\n" +
                    $"Acts(root)={database.acts?.Length ?? 0}," +
                    $"Areas={database.allAreas.Length}," +
                    $"Graphs={database.allGraphs.Length}," +
                    $"Layouts={database.allLayouts.Length}," +
                    $"NavData={database.allNavigationData.Length}");
                EditorUtility.DisplayDialog("Rebuild Complete", "CampaignDatabase rebuilt successfully", "OK");
            }
            catch (Exception e)
            {
                Debug.LogError($"CampaignDatabase rebuild exception: {e}");
                EditorUtility.DisplayDialog("Rebuild Error", "Exception occured. Check Console.", "OK");
            }
        }

        public static void Validate(CampaignDatabase database, bool strictSlugUniqueness)
        {
            List<string> errors = new List<string>();

            ValidateUniqueIds("database.acts", database.acts, errors);
            ValidateUniqueIds("database.allAreas", database.allAreas, errors);
            ValidateUniqueIds("database.allGraphs", database.allGraphs, errors);
            ValidateUniqueIds("database.allLayouts", database.allLayouts, errors);

            ValidateHierarchy(database, errors);

            if(errors.Count == 0)
            {
                Debug.Log("CampaignDatabase validation OK.");
                EditorUtility.DisplayDialog("Validate", "No issues found.", "OK");
            }
            else
            {
                Debug.LogError($"CampaignDatabase validation found {errors.Count} issue(s):\n- " + string.Join("\n- ", errors));
                EditorUtility.DisplayDialog("Validate", $"Found {errors.Count} issue(s). Check Console.", "OK");
            }
        }

        private static void ValidateHierarchy(CampaignDatabase database, List<string> errors)
        {
            if(database.acts is null || database.acts.Length == 0)
            {
                errors.Add("CampaignDatabase.acts is empty. Assign the root acts used for browsing.");

                return;
            }

            foreach(ActDef act in database.acts)
            {
                if(act == null)
                {
                    errors.Add("CampaignDatabase.acts has null act reference.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(act.id))
                {
                    errors.Add($"ActDef '{act.name}' has empty id.");
                }

                if(act.areas is null || act.areas.Length == 0)
                {
                    errors.Add($"Act '{act.id}' has no areas assigned.");

                    continue;
                }

                foreach(AreaDef area in act.areas)
                {
                    if(area == null)
                    {
                        errors.Add($"Act '{act.id}' has null area reference.");

                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(area.id))
                    {
                        errors.Add($"AreaDef '{area.name}' has empty id.");
                    }

                    if (area.graphs is null || area.graphs.Length == 0)
                    {
                        errors.Add($"Area '{area.id}' has no graphs assigned.");

                        continue;
                    }

                    foreach(GraphDef graph in area.graphs)
                    {
                        if(graph == null)
                        {
                            errors.Add($"Area '{area.id}' has null graph reference.");

                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(graph.id))
                        {
                            errors.Add($"GrapDef '{graph.name}' has empty id.");
                        }

                        if (graph.layouts is null || graph.layouts.Length == 0)
                        {
                            errors.Add($"Graph '{graph.id}' has no layouts assigned.");

                            continue;
                        }

                        foreach(LayoutDef layout in graph.layouts)
                        {
                            if(layout == null)
                            {
                                errors.Add($"Graph '{graph.id}' has null layout reference.");

                                continue;
                            }


                            if (string.IsNullOrWhiteSpace(layout.id))
                            {
                                errors.Add($"LayoutDef '{layout.name}' has empty id.");
                            }
                        }
                    }
                }
            }
        }

        private static void ValidateUniqueIds<T>(string label, IEnumerable<T> assets, List<string> errors) where T : DefBase
        {
            if(assets is null)
            {
                return;
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (T asset in assets)
            {
                if(asset is null)
                {
                    errors.Add($"{label} contains null.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(asset.id))
                {
                    errors.Add($"{label}: '{asset.name}' has empty id.");

                    continue;
                }

                if (!seen.Add(asset.id))
                {
                    errors.Add($"{label}: duplicate id '{asset.id}'.");
                }
            }
        }

        private static T[] FindAllAssetsOfType<T>() where T : ScriptableObject
        {
            string filter = $"t:{typeof(T).Name}";
            string[] guids = AssetDatabase.FindAssets(filter);

            List<T> list = new List<T>(guids.Length);
            foreach(string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if(asset is not null)
                {
                    list.Add(asset);
                }
            }

            return list.ToArray();
        }

        private static void MarkDirty(IEnumerable<ScriptableObject> objects)
        {
            if(objects is null)
            {
                return;
            }

            foreach (ScriptableObject @object in objects)
            {
                if(@object is null)
                {
                    continue;
                }

                EditorUtility.SetDirty(@object);
            }
        }
    }
}
#endif