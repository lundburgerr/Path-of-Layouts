using System.Collections.Generic;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public sealed class CollisionMap : MonoBehaviour
    {
        private static readonly Color32 _orangeColour = new Color32(255, 128, 0, 255);

        public bool IsBuilt {  get; private set; }

        [SerializeField] private int _minClusterSize = 8;

        private Vector2 _spawnPoint;

        public Vector2 GetSpawnPoint()
        {
            return _spawnPoint;
        }

        /// <summary>
        /// Scans the texture for exact orange pixels (255,128,0), finds 4-connected clusters,
        /// filters by minClusterSize, and returns the single centroid in pixel coordinates.
        /// Throws if none or more than one valid cluster is found.
        /// </summary>
        public void Build(Texture2D texture)
        {
            if (texture == null)
            {
                throw new System.ArgumentNullException(nameof(texture));
            }

            if (_minClusterSize < 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(_minClusterSize));
            }

            Color32[] pixels;
            try
            {
                pixels = texture.GetPixels32();
            }
            catch
            {
                throw new System.InvalidOperationException("Texture must be Read/Write Enabled (or otherwise readable) to scan pixels.");
            }

            int width = texture.width;
            int height = texture.height;
            int length = pixels.Length;

            var visited = new bool[length];
            var stack = new Stack<int>(256);

            // 4-connected neighbors
            int[] nx = { 1, -1, 0, 0 };
            int[] ny = { 0, 0, 1, -1 };

            bool found = false;
            Vector2 centroid = default;

            for (int i = 0; i < length; i++)
            {
                if (visited[i])
                {
                    continue;
                }
                if (!IsExactOrange(pixels[i]))
                {
                    continue;
                }

                visited[i] = true;
                stack.Clear();
                stack.Push(i);

                int count = 0;
                double sumX = 0.0;
                double sumY = 0.0;

                while (stack.Count > 0)
                {
                    int idx = stack.Pop();
                    int x = idx % width;
                    int y = idx / width;

                    count++;
                    sumX += x;
                    sumY += y;

                    for (int k = 0; k < 4; k++)
                    {
                        int xx = x + nx[k];
                        int yy = y + ny[k];

                        if ((uint)xx >= (uint)width || (uint)yy >= (uint)height)
                        {
                            continue;
                        }

                        int nIdx = yy * width + xx;

                        if (visited[nIdx])
                        {
                            continue;
                        }
                        if (!IsExactOrange(pixels[nIdx]))
                        {
                            continue;
                        }

                        visited[nIdx] = true;
                        stack.Push(nIdx);
                    }
                }

                if (count < _minClusterSize)
                {
                    continue;
                }

                Vector2 tempCentroid = new Vector2((float)(sumX / count), (float)(sumY / count));

                if (!found)
                {
                    found = true;
                    centroid = tempCentroid;
                }
                else
                {
                    throw new System.InvalidOperationException(
                        $"Multiple spawn centroids found (at least {centroid} and {tempCentroid}). Expected exactly one.");
                }
            }

            if (!found)
            {
                throw new System.InvalidOperationException("No valid spawn centroid found (no cluster >= minClusterSize).");
            }

            _spawnPoint = centroid;

            IsBuilt = true;
        }

        private static bool IsExactOrange(in Color32 colour)
        {
            return
                colour.r == _orangeColour.r &&
                colour.g == _orangeColour.g &&
                colour.b == _orangeColour.b;
        }

        public void Clear()
        {
            IsBuilt = false;

            _spawnPoint = Vector2.zero;
        }
    }
}