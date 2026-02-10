using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace fireMCG.PathOfLayouts.IO
{
    public static class JsonFileStore
    {
        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            Formatting = Formatting.Indented,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static async Task<T> LoadOrCreateAsync<T>(string path, Func<T> defaultFactory)
        {
            if(defaultFactory == null)
            {
                throw new ArgumentNullException(nameof(defaultFactory));
            }

            if (!File.Exists(path))
            {
                T created = defaultFactory();
                await SaveAsync(path, created);

                return created;
            }

            string json = await File.ReadAllTextAsync(path, Encoding.UTF8);
            T data = JsonConvert.DeserializeObject<T>(json, JsonSettings);

            if(data == null)
            {
                T created = defaultFactory();
                await SaveAsync(path, created);

                return created;
            }

            return data;
        }

        public static async Task SaveAsync<T>(string path, T data)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonConvert.SerializeObject(data, JsonSettings);

            string tempPath = path + ".tmp";

            await File.WriteAllTextAsync(tempPath, json, Encoding.UTF8);

            if (File.Exists(path))
            {
                string bakPath = path + "back";
                File.Replace(tempPath, path, bakPath, ignoreMetadataErrors: true);

                return;
            }
        }
    }
}