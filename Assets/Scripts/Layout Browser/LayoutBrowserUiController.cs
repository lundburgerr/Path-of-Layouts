using fireMCG.PathOfLayouts.Core;
using fireMCG.PathOfLayouts.IO;
using fireMCG.PathOfLayouts.LayoutBrowser.Ui;
using fireMCG.PathOfLayouts.Layouts;
using fireMCG.PathOfLayouts.Messaging;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine;
using fireMCG.PathOfLayouts.Campaign;

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
            Assert.IsNotNull(_actsMenuRoot);
            Assert.IsNotNull(_areaMenuRoot);
            Assert.IsNotNull(_graphGridRoot);
            Assert.IsNotNull(_layoutGridRoot);
            Assert.IsNotNull(_backButton);
            Assert.IsNotNull(_areaMenuContent);
            Assert.IsNotNull(_graphGridContent);
            Assert.IsNotNull(_layoutGridContent);
            Assert.IsNotNull(_areaCardPrefab);
            Assert.IsNotNull(_graphCardPrefab);
            Assert.IsNotNull(_layoutCardPrefab);
        }

        private void OnEnable()
        {
            RegisterMessageListeners();
        }

        private void OnDisable()
        {
            UnregisterMessageListeners();
        }

        private void RegisterMessageListeners()
        {
            MessageBusManager.Instance.Subscribe<OnAppStateChanged>(OnAppStateChanged);
        }

        private void UnregisterMessageListeners()
        {
            MessageBusManager.Instance.Unsubscribe<OnAppStateChanged>(OnAppStateChanged);
        }

        private void OnAppStateChanged(OnAppStateChanged message)
        {
            // Avoid clearing Ui when coming back from gameplay in order to resume at the same position.
            if(message.PreviousState == StateController.AppState.Gameplay)
            {
                return;
            }

            if(message.NewState == StateController.AppState.LayoutBrowser)
            {
                ResetUi();
            }
        }

        public void OpenMainMenu()
        {
            OnAppStateChangeRequest message = new OnAppStateChangeRequest(StateController.AppState.MainMenu);
            MessageBusManager.Instance.Publish(message);

            ResetUi();
        }

        public void SelectId(string id)
        {
            switch (_currentView)
            {
                case View.Acts:
                    _selectedActId = id;
                    PopulateAreaWindow();
                    break;

                case View.Areas:
                    _selectedAreaId = id;
                    PopulateGraphWindow();
                    break;

                case View.Graphs:
                    _selectedGraphId = id;
                    PopulateLayoutWindow();
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
                case View.Areas:
                    MessageBusManager.Instance.Publish(new LoadRandomGraphMessage(id));
                    break;

                case View.Graphs:
                    MessageBusManager.Instance.Publish(new LoadRandomLayoutMessage(id));
                    break;

                case View.Layouts:
                    MessageBusManager.Instance.Publish(new LoadTargetLayoutMessage(id));
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
                    Show(View.Graphs);
                    break;

                case View.Graphs:
                    _selectedAreaId = null;
                    ClearChildren(_graphGridContent);
                    Show(View.Areas);
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

        private void PopulateAreaWindow()
        {
            Show(View.Areas);

            IReadOnlyList<AreaDef> areas = Bootstrap.Instance.CampaignDatabase.GetAct(_selectedActId).areas;

            foreach(AreaDef area in areas)
            {
                AreaCard card = Instantiate(_areaCardPrefab, _areaMenuContent);
                card.Initialize(SelectId, PlayId, area.id);
            }
        }

        private void PopulateGraphWindow()
        {
            Show(View.Graphs);

            IReadOnlyList<GraphDef> graphs = Bootstrap.Instance.CampaignDatabase.GetArea(_selectedAreaId).graphs;

            foreach (GraphDef graph in graphs)
            {
                // string renderPath = StreamingPathResolver.GetGraphRenderFilePath(_selectedActId, _selectedAreaId, graph.id);
                // Texture2D texture = TextureFileLoader.LoadPng(renderPath, FilterMode.Bilinear);

                GraphCard card = Instantiate(_graphCardPrefab, _graphGridContent);
                // card.Initialize(SelectId, PlayId, graph.id, texture);
            }
        }

        private void PopulateLayoutWindow()
        {
            Show(View.Layouts);

            IReadOnlyList<LayoutDef> layouts = Bootstrap.Instance.CampaignDatabase.GetGraph(_selectedGraphId).layouts;

            foreach (LayoutDef layout in layouts)
            {
                LayoutCard card = Instantiate(_layoutCardPrefab, _layoutGridContent);
                card.Initialize(SelectId, PlayId, null, _selectedActId, _selectedAreaId, _selectedGraphId, layout.id);
            }
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
            if (!parent)
            {
                return;
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}