using System;
using System.Collections.Generic;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Campaign
{
    [CreateAssetMenu(menuName = "Path of Layouts/Campaign/Campaign Database")]
    public sealed class CampaignDatabase : ScriptableObject
    {
        public ActDef[] acts = Array.Empty<ActDef>();

        public AreaDef[] allAreas = Array.Empty<AreaDef>();
        public GraphDef[] allGraphs = Array.Empty<GraphDef>();
        public LayoutDef[] allLayouts = Array.Empty<LayoutDef>();
        public NavigationDataAsset[] allNavigationData = Array.Empty<NavigationDataAsset>();

        private Dictionary<string, ActDef> _actById;
        private Dictionary<string, AreaDef> _areaById;
        private Dictionary<string, GraphDef> _graphById;
        private Dictionary<string, LayoutDef> _layoutById;

        private Dictionary<string, ActDef> _areaToAct;
        private Dictionary<string, AreaDef> _graphToArea;
        private Dictionary<string, GraphDef> _layoutToGraph;

        public bool IsIndexed => _layoutById != null;

        private void OnValidate()
        {
            BuildRuntimeIndex();
        }

        public void BuildRuntimeIndex()
        {
            _actById = new Dictionary<string, ActDef>(StringComparer.Ordinal);
            _areaById = new Dictionary<string, AreaDef>(StringComparer.Ordinal);
            _graphById = new Dictionary<string, GraphDef>(StringComparer.Ordinal);
            _layoutById = new Dictionary<string, LayoutDef>(StringComparer.Ordinal);

            _areaToAct = new Dictionary<string, ActDef>(StringComparer.Ordinal);
            _graphToArea = new Dictionary<string, AreaDef>(StringComparer.Ordinal);
            _layoutToGraph = new Dictionary<string, GraphDef>(StringComparer.Ordinal);

            IndexById(_actById, acts);
            IndexById(_areaById, allAreas);
            IndexById(_graphById, allGraphs);
            IndexById(_layoutById, allLayouts);

            BuildParentMaps();
        }

        public void IndexById<T>(Dictionary<string, T> dictionary, IEnumerable<T> items) where T : DefBase
        {
            if(items is null)
            {
                return;
            }

            foreach(var item in items)
            {
                if(item is null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.id))
                {
                    continue;
                }

                dictionary.TryAdd(item.id, item);
            }
        }

        private void BuildParentMaps()
        {
            if (acts is null)
            {
                return;
            }

            foreach (ActDef act in acts)
            {
                if (act == null)
                {
                    continue;
                }

                AreaDef[] areas = act.areas;
                if (areas is null)
                {
                    continue;
                }

                foreach (AreaDef area in areas)
                {
                    if (area == null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(area.id))
                    {
                        _areaToAct[area.id] = act;
                    }

                    GraphDef[] graphs = area.graphs;
                    if (areas is null)
                    {
                        continue;
                    }

                    foreach (GraphDef graph in graphs)
                    {
                        if (graph == null)
                        {
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(graph.id))
                        {
                            _graphToArea[graph.id] = area;
                        }

                        LayoutDef[] layouts = graph.layouts;
                        if (layouts is null)
                        {
                            continue;
                        }

                        foreach (LayoutDef layout in layouts)
                        {
                            if (layout == null)
                            {
                                continue;
                            }

                            if (!string.IsNullOrWhiteSpace(layout.id))
                            {
                                _layoutToGraph[layout.id] = graph;
                            }
                        }
                    }
                }
            }
        }

        public ActDef GetAct(string id) => _actById[id];
        public AreaDef GetArea(string id) => _areaById[id];
        public GraphDef GetGraph(string id) => _graphById[id];
        public LayoutDef GetLayout(string id) => _layoutById[id];

        public bool TryGetLayout(string id, out LayoutDef layout)
        {
            layout = null;

            return _layoutById is not null && _layoutById.TryGetValue(id, out layout);
        }

        public ActDef GetParentAct(string areaId)
        {
            if(!_areaToAct.TryGetValue(areaId, out ActDef act))
            {
                return null;
            }

            return act;
        }

        public AreaDef GetParentArea(string graphId)
        {
            if (!_graphToArea.TryGetValue(graphId, out AreaDef area))
            {
                return null;
            }

            return area;
        }

        public GraphDef GetParentGraph(string layoutId)
        {
            if (!_layoutToGraph.TryGetValue(layoutId, out GraphDef graph))
            {
                return null;
            }

            return graph;
        }

        public AreaDef GetParentAreaFromLayout(string layoutId)
        {
            GraphDef graph = GetParentGraph(layoutId);
            if (graph == null)
            {
                return null;
            }

            return GetParentArea(graph.id);
        }
    }
}