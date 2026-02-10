using fireMCG.PathOfLayouts.IO;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace fireMCG.PathOfLayouts.Manifest
{
    public sealed class CampaignManifestService
    {
        public CampaignManifest Manifest { get; private set; }

        public async Task LoadManifestAsync()
        {
            string json = await ReadStreamingAssetsTextAsync(StreamingPathResolver.GetManifestFilePath());
            CampaignManifest manifest = JsonConvert.DeserializeObject<CampaignManifest>(json);

            if(manifest is null)
            {
                throw new Exception($"CampaignManifestService.LoadManifestAsync error,JSON deserialized to null.");
            }

            Manifest = manifest;
        }

        private static async Task<string> ReadStreamingAssetsTextAsync(string path)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                using UnityWebRequest request = UnityWebRequest.Get(path);
                UnityWebRequestAsyncOperation op = request.SendWebRequest();

                while (!op.isDone)
                {
                    await Task.Yield();
                }

                if(request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception(
                        $"CampaignManifestService.ReadStreamingAssetsTextAsync error");
                }

                return request.downloadHandler.text;
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"CampaignManifestService.ReadStreamingAssetsTextAsync error, manifest not found at {path}");
            }

            return await File.ReadAllTextAsync(path);
        }
    }
}