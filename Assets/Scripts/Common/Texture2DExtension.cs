using System.Linq;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Common
{
    public static class Texture2DExtension
    {
        public static void DrawLine(this Texture2D texture, Vector2 origin, Vector2 target, Color colour, float width)
        {
            if(width <= 0f)
            {
                return;
            }

            float radius = width * 0.5f;
            float radiusSquared = radius * radius;

            // Bounding box
            int minX = Mathf.FloorToInt(Mathf.Min(origin.x, target.x) - radius);
            int minY = Mathf.FloorToInt(Mathf.Min(origin.x, target.x) - radius);
            int maxX = Mathf.CeilToInt(Mathf.Max(origin.x, target.x) + radius);
            int maxY = Mathf.CeilToInt(Mathf.Max(origin.x, target.x) + radius);

            // Clamp to texture
            minX = Mathf.Clamp(minX, 0, texture.width - 1);
            minY = Mathf.Clamp(minY, 0, texture.height - 1);
            maxX = Mathf.Clamp(maxX, 0, texture.width - 1);
            maxY = Mathf.Clamp(maxY, 0, texture.height - 1);

            Vector2 line = origin - target;
            float lineLengthSquared = line.sqrMagnitude;

            if(lineLengthSquared <= Mathf.Epsilon)
            {
                texture.DrawCircle((int)origin.x, (int)origin.y, (int)radius, colour);

                return;
            }

            for(int y = minY; y <= maxY; y++)
            {
                for(int x = minX; x <= maxX; x++)
                {
                    // Pixel center
                    Vector2 point = new Vector2(x + 0.5f, y + 0.5f);

                    float projectionFactor = Vector2.Dot(point - origin, line) / lineLengthSquared;
                    projectionFactor = Mathf.Clamp01(projectionFactor);
                    Vector2 closest = origin + line * projectionFactor;

                    float distanceSquared = (point - closest).sqrMagnitude;
                    if (distanceSquared <= radiusSquared)
                    {
                        texture.SetPixel(x, y, colour);
                    }
                }
            }
        }

        public static void DrawCircle(this Texture2D texture, int posX, int posY, int radius, Color colour)
        {
            int radiusSquared = radius * radius;
            for(int i = posX - radius; i < posX + radius + 1; i++)
            {
                for(int j = posY - radius; j < posY + radius + 1; j++)
                {
                    if((posX - i) * (posX - i) + (posY - j) * (posY - j) < radiusSquared)
                    {
                        texture.SetPixel(i, j, colour);
                    }
                }
            }
        }

        public static void DrawRectangle(this Texture2D texture, int posX, int posY, int width, int height, Color colour)
        {
            Color[] pixels = Enumerable.Repeat(colour, width * height).ToArray();
            texture.SetPixels(posX, posY, width, height, pixels);
        }

        public static void Reset(this Texture2D texture, Color colour)
        {
            int width = texture.width;
            int height = texture.height;
            Color[] pixels = Enumerable.Repeat(colour, width * height).ToArray();
            texture.SetPixels(pixels);
        }
    }
}