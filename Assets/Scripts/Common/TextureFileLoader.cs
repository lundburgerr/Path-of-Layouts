using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Common
{
    public static class TextureFileLoader
    {
        public static async Task<Texture2D> LoadPngAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            byte[] bytes = await File.ReadAllBytesAsync(filePath);
            if (bytes is null || bytes.Length == 0)
            {
                return null;
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false, linear: false);
            bool ok = texture.LoadImage(bytes, markNonReadable: false);

            if (!ok)
            {
                return null;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.anisoLevel = 0;

            return texture;
        }
    }
}