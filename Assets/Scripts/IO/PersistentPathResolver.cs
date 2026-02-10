using System.IO;
using UnityEngine;

namespace fireMCG.PathOfLayouts.IO
{
    public static class PersistentPathResolver
    {
        public const string SRS_FOLDER = "srs";
        public const string SETTINGS_FOLDER = "settings";
        public const string SRS_SAVE_FILE_NAME = "srs_data.json";
        public const string LAYOUT_PARAMS_FILE_NAME = "layout_params.json";
        public const string GAMEPLAY_SETTINGS_FILE_NAME = "gameplay_settings.json";

        public static string GetRootPath()
        {
            return Application.persistentDataPath;
        }

        public static string GetSrsFolderPath()
        {
            return Path.Combine(GetRootPath(), SRS_FOLDER);
        }

        public static string GetSettingsFolderPath()
        {
            return Path.Combine(GetRootPath(), SETTINGS_FOLDER);
        }

        public static string GetSrsFilePath()
        {
            return Path.Combine(GetSrsFolderPath(), SRS_SAVE_FILE_NAME);
        }

        public static string GetLayoutParamsFilePath()
        {
            return Path.Combine(GetSettingsFolderPath(), LAYOUT_PARAMS_FILE_NAME);
        }

        public static string GetGameplaySettingsFilePath()
        {
            return Path.Combine(GetSettingsFolderPath(), GAMEPLAY_SETTINGS_FILE_NAME);
        }

        public static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(GetSrsFolderPath());
            Directory.CreateDirectory(GetSettingsFolderPath());
        }
    }
}