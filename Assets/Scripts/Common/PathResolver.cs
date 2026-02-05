using System.IO;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Common
{
    public static class PathResolver
    {
        public const string ROOT_FOLDER = "Campaign";
        public const string MANIFEST_FILE_NAME = "campaign_manifest.json";
        public const string SRS_SAVE_FILE_NAME = "srs_save.json";
        public const string LAYOUT_PARAMS_FILE_NAME = "layout_params.json";
        public const string LAYOUT_SUFFIX = ".layout.png";
        public const string COLLISION_MAP_SUFFIX = ".collision.png";
        public const string GRAPH_RENDER_SUFFIX = ".render.png";

        public static string GetRootPath()
        {
            return Path.Combine(Application.streamingAssetsPath, ROOT_FOLDER);
        }

        public static string GetManifestFilePath()
        {
            return Path.Combine(GetRootPath(), MANIFEST_FILE_NAME);
        }

        public static string GetActPath(string actId)
        {
            return Path.Combine(GetRootPath(), actId);
        }

        public static string GetAreaPath(string actId, string areaId)
        {
            return Path.Combine(GetActPath(actId), areaId);
        }

        public static string GetGraphFolderPath(string actId, string areaId, string graphId)
        {
            return Path.Combine(GetAreaPath(actId, areaId), graphId);
        }

        public static string GetGraphFilePath(string actId, string areaId, string graphId, string fileExtension)
        {
            return Path.Combine(GetAreaPath(actId, areaId), graphId + fileExtension);
        }

        public static string GetGraphRenderFilePath(string actId, string areaId, string graphId)
        {
            return Path.Combine(GetAreaPath(actId, areaId), graphId + GRAPH_RENDER_SUFFIX);
        }

        public static string GetLayoutFilePath(string actId, string areaId, string graphId, string layoutId)
        {
            return Path.Combine(GetGraphFolderPath(actId, areaId, graphId), layoutId + LAYOUT_SUFFIX);
        }

        public static string GetCollisionMapFilePath(string actId, string areaId, string graphId, string layoutId)
        {
            return Path.Combine(GetGraphFolderPath(actId, areaId, graphId), layoutId + COLLISION_MAP_SUFFIX);
        }
    }
}