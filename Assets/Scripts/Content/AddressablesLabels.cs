namespace fireMCG.PathOfLayouts.Content
{
    public static class AddressablesLabels
    {
        // Graph renders are downloaded per AREA (when browsing/selecting an area).
        public const string AREA_GRAPH_RENDERS_PREFIX = "area_graphRenders__";

        // Layout images are downloaded per GRAPH (when browsing/selecting a graph).
        public const string GRAPH_LAYOUT_IMAGES_PREFIX = "graph_layoutImages__";

        public static string GetAreaGraphRendersLabel(string areaId)
        {
            if (string.IsNullOrWhiteSpace(areaId))
            {
                return string.Empty;
            }

            return AREA_GRAPH_RENDERS_PREFIX + areaId;
        }

        public static string GetGraphLayoutImagesLabel(string graphId)
        {
            if (string.IsNullOrWhiteSpace(graphId))
            {
                return string.Empty;
            }

            return GRAPH_LAYOUT_IMAGES_PREFIX + graphId;
        }
    }
}