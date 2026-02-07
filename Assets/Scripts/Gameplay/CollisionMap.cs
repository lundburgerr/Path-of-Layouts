using System;
using System.Collections.Generic;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public sealed class CollisionMap : MonoBehaviour
    {
        // Walkable == "not Blocked"
        public enum CellType : byte
        {
            Blocked = 0,
            Floor = 1,
            NodeOrange = 2,
            NodeYellow = 3,
            NodeGreen = 4,
        }

        [Header("Build Settings")]
        [Range(1, 16)] public int sampleStep = 2;

        [Header("Walkable (black) threshold")]
        [Range(0f, 0.35f)] public float blackMax = 0.10f;

        [Header("Marker detection")]
        [Range(0, 160)] public int markerTolerance = 20;
        [Range(1, 200)] public int minClusterSize = 12;

        // Marker reference colors
        public static readonly Color32 ORANGE = new Color32(255, 165, 0, 255);
        public static readonly Color32 YELLOW = new Color32(255, 255, 0, 255);
        public static readonly Color32 GREEN = new Color32(0, 255, 0, 255);

        public bool IsBuilt { get; private set; }
        public int TextureWidth { get; private set; }
        public int TextureHeight { get; private set; }
        public int Step { get; private set; }
        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }

        public Vector2Int? OrangeNodeGrid { get; private set; }
        public Vector2Int? YellowNodeGrid { get; private set; }

        public IReadOnlyList<Vector2Int> GreenNodesGrid
        {
            get
            {
                return _greenNodeCentroids;
            }
        }

        public event Action<CellType, Vector2Int> OnNodeEnteredGrid;

        private CellType[] _gridCells; // flattened: [x + y * GridWidth]
        private readonly List<Vector2Int> _greenNodeCentroids = new List<Vector2Int>(16);

        // Cached threshold to avoid float math per sample
        private byte _blackMaxByte;

        public void Clear()
        {
            IsBuilt = false;

            TextureWidth = 0;
            TextureHeight = 0;

            Step = 0;
            GridWidth = 0;
            GridHeight = 0;

            OrangeNodeGrid = null;
            YellowNodeGrid = null;

            _greenNodeCentroids.Clear();
            _gridCells = null;
        }

        /// <summary>
        /// Builds a grid-based collision + marker map from a processed screenshot ".collision.png" texture.
        /// The source texture is not stored; only baked results are kept.
        /// </summary>
        public void Build(Texture2D cleanedTexture)
        {
            Clear();

            if (cleanedTexture == null)
            {
                Debug.LogError("CollisionMap.Build: cleanedTexture is null.");

                return;
            }

            InitializeBuildDimensions(cleanedTexture);

            Color32[] pixels = cleanedTexture.GetPixels32();

            ClassifyGridCells(pixels);
            ComputeMarkerCentroidsFromGrid();

            IsBuilt = true;
        }

        public bool IsInsideGrid(Vector2Int gridPosition)
        {
            if (gridPosition.x < 0)
            {
                return false;
            }

            if (gridPosition.y < 0)
            {
                return false;
            }

            if (gridPosition.x >= GridWidth)
            {
                return false;
            }

            if (gridPosition.y >= GridHeight)
            {
                return false;
            }

            return true;
        }

        public CellType GetCellGrid(Vector2Int gridPosition)
        {
            if (!IsBuilt)
            {
                return CellType.Blocked;
            }

            if (!IsInsideGrid(gridPosition))
            {
                return CellType.Blocked;
            }

            return _gridCells[gridPosition.x + (gridPosition.y * GridWidth)];
        }

        /// <summary>
        /// Walkable means "not Blocked" (nodes are walkable too).
        /// </summary>
        public bool IsWalkableGrid(Vector2Int gridPosition)
        {
            return GetCellGrid(gridPosition) != CellType.Blocked;
        }

        public void NotifyEnteredGridCell(Vector2Int gridPosition)
        {
            CellType cell = GetCellGrid(gridPosition);

            if (cell == CellType.NodeOrange || cell == CellType.NodeYellow || cell == CellType.NodeGreen)
            {
                Action<CellType, Vector2Int> handler = OnNodeEnteredGrid;
                if (handler != null)
                {
                    handler.Invoke(cell, gridPosition);
                }
            }
        }

        private void InitializeBuildDimensions(Texture2D cleanedTexture)
        {
            Step = Mathf.Max(1, sampleStep);

            TextureWidth = cleanedTexture.width;
            TextureHeight = cleanedTexture.height;

            GridWidth = (TextureWidth + Step - 1) / Step;
            GridHeight = (TextureHeight + Step - 1) / Step;

            _gridCells = new CellType[GridWidth * GridHeight];

            _blackMaxByte = (byte)Mathf.RoundToInt(blackMax * 255f);

            _greenNodeCentroids.Clear();
            OrangeNodeGrid = null;
            YellowNodeGrid = null;
        }

        private void ClassifyGridCells(Color32[] pixels)
        {
            for (int gridY = 0; gridY < GridHeight; gridY++)
            {
                int samplePixelY = gridY * Step;
                if (samplePixelY >= TextureHeight)
                {
                    continue;
                }

                for (int gridX = 0; gridX < GridWidth; gridX++)
                {
                    int samplePixelX = gridX * Step;
                    if (samplePixelX >= TextureWidth)
                    {
                        continue;
                    }

                    MarkerPresence markers = DetectMarkersInBlock(pixels, samplePixelX, samplePixelY);
                    bool isBlackWalkable = IsBlackWalkableAtSample(pixels, samplePixelX, samplePixelY);

                    CellType cellType = DetermineCellType(markers, isBlackWalkable);

                    int cellIndex = gridX + (gridY * GridWidth);
                    _gridCells[cellIndex] = cellType;
                }
            }
        }

        private void ComputeMarkerCentroidsFromGrid()
        {
            List<Vector2Int> orangeCentroids = FindMarkerCentroids(CellType.NodeOrange, minClusterSize);
            List<Vector2Int> yellowCentroids = FindMarkerCentroids(CellType.NodeYellow, minClusterSize);
            List<Vector2Int> greenCentroids = FindMarkerCentroids(CellType.NodeGreen, minClusterSize);

            if (orangeCentroids.Count > 0)
            {
                OrangeNodeGrid = orangeCentroids[0];
            }

            if (yellowCentroids.Count > 0)
            {
                YellowNodeGrid = yellowCentroids[0];
            }

            _greenNodeCentroids.Clear();
            _greenNodeCentroids.AddRange(greenCentroids);
        }

        private readonly struct MarkerPresence
        {
            public readonly bool HasOrange;
            public readonly bool HasYellow;
            public readonly bool HasGreenOnly;

            public MarkerPresence(bool hasOrange, bool hasYellow, bool hasGreenOnly)
            {
                HasOrange = hasOrange;
                HasYellow = hasYellow;
                HasGreenOnly = hasGreenOnly;
            }
        }

        private MarkerPresence DetectMarkersInBlock(Color32[] pixels, int startX, int startY)
        {
            bool hasOrange = BlockContainsColor(
                pixels,
                TextureWidth,
                TextureHeight,
                startX,
                startY,
                Step,
                ORANGE,
                markerTolerance);

            bool hasYellow = BlockContainsColor(
                pixels,
                TextureWidth,
                TextureHeight,
                startX,
                startY,
                Step,
                YELLOW,
                markerTolerance);

            bool hasGreen = BlockContainsColor(
                pixels,
                TextureWidth,
                TextureHeight,
                startX,
                startY,
                Step,
                GREEN,
                markerTolerance);

            // Priority: orange/yellow override green
            bool hasGreenOnly = hasGreen && !hasOrange && !hasYellow;

            return new MarkerPresence(hasOrange, hasYellow, hasGreenOnly);
        }

        private bool IsBlackWalkableAtSample(Color32[] pixels, int pixelX, int pixelY)
        {
            int index = (pixelY * TextureWidth) + pixelX;
            Color32 sample = pixels[index];

            if (sample.r <= _blackMaxByte &&
                sample.g <= _blackMaxByte &&
                sample.b <= _blackMaxByte)
            {
                return true;
            }

            return false;
        }

        private static CellType DetermineCellType(MarkerPresence markers, bool isBlackWalkable)
        {
            if (markers.HasOrange)
            {
                return CellType.NodeOrange;
            }

            if (markers.HasYellow)
            {
                return CellType.NodeYellow;
            }

            if (markers.HasGreenOnly)
            {
                return CellType.NodeGreen;
            }

            if (isBlackWalkable)
            {
                return CellType.Floor;
            }

            return CellType.Blocked;
        }

        private static bool BlockContainsColor(
            Color32[] pixels,
            int textureWidth,
            int textureHeight,
            int startX,
            int startY,
            int blockSize,
            Color32 targetColor,
            int tolerance)
        {
            int endX = Mathf.Min(startX + blockSize, textureWidth);
            int endY = Mathf.Min(startY + blockSize, textureHeight);

            for (int y = startY; y < endY; y++)
            {
                int rowStartIndex = y * textureWidth;

                for (int x = startX; x < endX; x++)
                {
                    Color32 pixel = pixels[rowStartIndex + x];

                    if (IsColorWithinTolerance(pixel, targetColor, tolerance))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsColorWithinTolerance(Color32 a, Color32 b, int tolerance)
        {
            if (Mathf.Abs(a.r - b.r) > tolerance)
            {
                return false;
            }

            if (Mathf.Abs(a.g - b.g) > tolerance)
            {
                return false;
            }

            if (Mathf.Abs(a.b - b.b) > tolerance)
            {
                return false;
            }

            return true;
        }

        private readonly struct ClusterStats
        {
            public readonly int CellCount;
            public readonly long SumX;
            public readonly long SumY;

            public ClusterStats(int cellCount, long sumX, long sumY)
            {
                CellCount = cellCount;
                SumX = sumX;
                SumY = sumY;
            }

            public Vector2Int CentroidInt
            {
                get
                {
                    if (CellCount == 0)
                    {
                        return default;
                    }

                    int cx = (int)(SumX / CellCount);
                    int cy = (int)(SumY / CellCount);

                    return new Vector2Int(cx, cy);
                }
            }
        }

        private List<Vector2Int> FindMarkerCentroids(CellType markerType, int minSize)
        {
            bool[,] visited = new bool[GridWidth, GridHeight];

            List<(Vector2Int centroid, int size)> clusters = new List<(Vector2Int centroid, int size)>();

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    if (!IsMarkerSeed(markerType, visited, x, y))
                    {
                        continue;
                    }

                    Vector2Int seed = new Vector2Int(x, y);
                    ClusterStats stats = FloodFillMarkerCluster(markerType, visited, seed);

                    if (stats.CellCount >= minSize)
                    {
                        clusters.Add((stats.CentroidInt, stats.CellCount));
                    }
                }
            }

            clusters.Sort((a, b) => b.size.CompareTo(a.size));

            List<Vector2Int> centroids = new List<Vector2Int>(clusters.Count);
            for (int i = 0; i < clusters.Count; i++)
            {
                centroids.Add(clusters[i].centroid);
            }

            return centroids;
        }

        private bool IsMarkerSeed(CellType markerType, bool[,] visited, int x, int y)
        {
            if (visited[x, y])
            {
                return false;
            }

            int index = x + (y * GridWidth);
            if (_gridCells[index] != markerType)
            {
                return false;
            }

            return true;
        }

        private ClusterStats FloodFillMarkerCluster(CellType markerType, bool[,] visited, Vector2Int seed)
        {
            Queue<Vector2Int> queue = new Queue<Vector2Int>(256);

            queue.Enqueue(seed);
            visited[seed.x, seed.y] = true;

            int cellCount = 0;
            long sumX = 0;
            long sumY = 0;

            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();

                cellCount++;
                sumX += cell.x;
                sumY += cell.y;

                EnqueueMarkerNeighbors(markerType, cell, visited, queue);
            }

            return new ClusterStats(cellCount, sumX, sumY);
        }

        private void EnqueueMarkerNeighbors(CellType markerType, Vector2Int cell, bool[,] visited, Queue<Vector2Int> queue)
        {
            for (int ny = cell.y - 1; ny <= cell.y + 1; ny++)
            {
                for (int nx = cell.x - 1; nx <= cell.x + 1; nx++)
                {
                    if (nx == cell.x && ny == cell.y)
                    {
                        continue;
                    }

                    if ((uint)nx >= (uint)GridWidth || (uint)ny >= (uint)GridHeight)
                    {
                        continue;
                    }

                    if (visited[nx, ny])
                    {
                        continue;
                    }

                    int neighborIndex = nx + (ny * GridWidth);
                    if (_gridCells[neighborIndex] != markerType)
                    {
                        continue;
                    }

                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
    }
}