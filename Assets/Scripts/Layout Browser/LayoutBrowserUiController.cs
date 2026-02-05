using fireMCG.PathOfLayouts.Common;
using fireMCG.PathOfLayouts.LayoutBrowser.Ui;
using fireMCG.PathOfLayouts.Manifest;
using fireMCG.PathOfLayouts.Messaging;
using fireMCG.PathOfLayouts.System;
using System.Collections.Generic;
using UnityEngine;

namespace fireMCG.PathOfLayouts.Ui
{
    public sealed class LayoutBrowserUiController : MonoBehaviour
    {
        private enum View
        {
            Acts,
            Areas,
            Graphs,
            Layouts
        }

        [SerializeField] private GameObject _actsMenuRoot;
        [SerializeField] private GameObject _areaMenuRoot;
        [SerializeField] private GameObject _graphGridRoot;
        [SerializeField] private GameObject _layoutGridRoot;
        [SerializeField] private GameObject _backButton;

        [SerializeField] private RectTransform _areaMenuContent;
        [SerializeField] private RectTransform _graphGridContent;
        [SerializeField] private RectTransform _layoutGridContent;

        [SerializeField] private AreaCard _areaCardPrefab;
        [SerializeField] private GraphCard _graphCardPrefab;
        [SerializeField] private LayoutCard _layoutCardPrefab;

        private View _currentView = View.Acts;
        private string _selectedActId = null;
        private string _selectedAreaId = null;
        private string _selectedGraphId = null;
        private string _selectedLayoutId = null;

        private void Awake()
        {
            ResetUi();
        }

        public void OpenMainMenu()
        {
            OnAppStateChangeRequest message = new OnAppStateChangeRequest(StateController.AppState.MainMenu);
            MessageBusManager.Resolve.Publish(message);

            ResetUi();
        }

        public void SelectId(string id)
        {
            switch (_currentView)
            {
                case View.Acts:
                    _selectedActId = id;
                    OpenAreaWindow();
                    break;

                case View.Areas:
                    _selectedAreaId = id;
                    OpenGraphWindow();
                    break;

                case View.Graphs:
                    _selectedGraphId = id;
                    OpenLayoutWindow();
                    break;

                case View.Layouts:
                    _selectedLayoutId = id;
                    OpenLayoutSettings();
                    break;

                default:
                    // Error Message
                    ResetUi();
                    break;
            }
        }

        public void PlayId(string id)
        {
            switch (_currentView)
            {
                case View.Graphs:
                    break;

                case View.Layouts:
                    break;

                default:
                    // Error Message
                    ResetUi();
                    break;
            }
        }

        public void Back()
        {
            switch (_currentView)
            {
                case View.Layouts:
                    _selectedGraphId = null;
                    _selectedLayoutId = null;
                    ClearChildren(_layoutGridContent);
                    OpenGraphWindow();
                    break;

                case View.Graphs:
                    _selectedAreaId = null;
                    ClearChildren(_graphGridContent);
                    OpenAreaWindow();
                    break;

                case View.Areas:
                    _selectedActId = null;
                    ClearChildren(_areaMenuContent);
                    Show(View.Acts);
                    break;

                case View.Acts:
                default:
                    break;
            }
        }

        private void OpenAreaWindow()
        {
            Show(View.Areas);

            IReadOnlyList<AreaEntry> areas =Bootstrap.Instance.ManifestService.Manifest.GetAreas(_selectedActId);

            foreach(AreaEntry area in areas)
            {
                AreaCard card = Instantiate(_areaCardPrefab, _areaMenuContent);
                card.Initialize(SelectId, area.areaId);
            }
        }

        private async void OpenGraphWindow()
        {
            Show(View.Graphs);

            IReadOnlyList<GraphEntry> graphs = Bootstrap.Instance.ManifestService.Manifest
                .GetGraphs(_selectedActId, _selectedAreaId);

            foreach (GraphEntry graph in graphs)
            {
                string renderPath = PathResolver.GetGraphRenderFilePath(_selectedActId, _selectedAreaId, graph.graphId);
                Texture2D texture = await TextureFileLoader.LoadPngAsync(renderPath);

                GraphCard card = Instantiate(_graphCardPrefab, _graphGridContent);
                card.Initialize(SelectId, PlayId, graph.graphId, texture);
            }
        }

        private void OpenLayoutWindow()
        {
            Show(View.Layouts);
        }

        private void OpenLayoutSettings()
        {

        }

        private void ResetUi()
        {
            _selectedActId = null;
            _selectedAreaId = null;
            _selectedGraphId = null;
            _selectedLayoutId = null;

            ClearChildren(_areaMenuContent);
            ClearChildren(_graphGridContent);
            ClearChildren(_layoutGridContent);

            Show(View.Acts);
        }

        private void Show(View view)
        {
            _currentView = view;

            _actsMenuRoot.SetActive(view == View.Acts);
            _areaMenuRoot.SetActive(view == View.Areas);
            _graphGridRoot.SetActive(view == View.Graphs);
            _layoutGridRoot.SetActive(view == View.Layouts);

            _backButton.SetActive(view != View.Acts);
        }

        private static void ClearChildren(RectTransform parent)
        {
            if (!parent) return;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}