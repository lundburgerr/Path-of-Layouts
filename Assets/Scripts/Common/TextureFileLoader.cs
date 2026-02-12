using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Common
{
    public static class TextureFileLoader
    {
        public static Texture2D LoadPng(string filePath, FilterMode filterMode)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            // byte[] bytes = await File.ReadAllBytesAsync(filePath);
            // if (bytes is null || bytes.Length == 0)
            // {
            //     return null;
            // }

            Image<Rgba32> resizedImage = LoadAndResizeNearest(filePath, 4096);
            resizedImage.Mutate(x => x.Flip(FlipMode.Vertical));
            byte[] bytes = new byte[resizedImage.Width * resizedImage.Height * 4];
            resizedImage.CopyPixelDataTo(bytes);

            Texture2D texture = new Texture2D(resizedImage.Width, resizedImage.Height, TextureFormat.RGBA32, mipChain: false, linear: false);
            texture.LoadRawTextureData(bytes);
            texture.Apply();

            texture.filterMode = filterMode;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.anisoLevel = 0;

            return texture;
        }

        public static Image<Rgba32> LoadAndResizeNearest(string filePath, int textureSizeCap)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            Image<Rgba32> img = Image.Load<Rgba32>(filePath);

            int width = img.Width;
            int height = img.Height;
            int maxDimension = Mathf.Max(width, height);
            if(maxDimension > textureSizeCap)
            {
                float scale = textureSizeCap / (float)maxDimension;
                int newWidth = Mathf.Max(1, Mathf.RoundToInt(width * scale));
                int newHeight = Mathf.Max(1, Mathf.RoundToInt(height * scale));

                img.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(newWidth, newHeight),
                    Sampler = KnownResamplers.NearestNeighbor,
                    Mode = ResizeMode.Stretch
                }));
            }

            return img;
        }

        public static Image<Rgba32> LoadAndResizeLanczos3(string filePath, int textureSizeCap)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            Image<Rgba32> img = Image.Load<Rgba32>(filePath);

            int width = img.Width;
            int height = img.Height;
            int maxDimension = Mathf.Max(width, height);
            if (maxDimension > textureSizeCap)
            {
                float scale = textureSizeCap / (float)maxDimension;
                int newWidth = Mathf.Max(1, Mathf.RoundToInt(width * scale));
                int newHeight = Mathf.Max(1, Mathf.RoundToInt(height * scale));

                img.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(newWidth, newHeight),
                    Sampler = KnownResamplers.Lanczos3,
                    Mode = ResizeMode.Stretch
                }));
            }

            return img;
        }
    }
}