using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OutwardModsCommunicatorMenu.Utility;
using SideLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Panels;

namespace OutwardModsCommunicatorMenu.Managers
{
    public class UIManager
    {
        private static UIManager _instance;
        public static UIManager Instance => _instance ??= new UIManager();

        private const string MENU_KEYBIND_NAME = "OMM_Menu_Toggle";
        
        public ManualLogSource Log { get; private set; }
        public UIBase MenuUI { get; private set; }

        private Harmony _harmony;
        private bool _isInitialized;
        private MainPanel _mainPanel;

        private UIManager() { }

        public void Initialize(ManualLogSource logSource)
        {
            if (_isInitialized) return;

            Log = logSource;
            LogMessage("Initializing UIManager...");

            KeybindingHelper.Register(MENU_KEYBIND_NAME, "Mods Communicator Menu");

            _harmony = new Harmony(OutwardModsCommunicatorMenu.OMCM.GUID + ".UIManager");
            _harmony.PatchAll(typeof(Patches.ResourcesPrefabManager_Load));

            _isInitialized = true;
            LogMessage("UIManager initialized");
        }

        public void OnUpdate()
        {
            if (KeybindingHelper.IsKeyDown(MENU_KEYBIND_NAME))
            {
                MenuVisibilityManager.Instance.Toggle();
            }

            if (MenuVisibilityManager.Instance.IsVisible)
            {
                UI.EventPublishingPanelBuilder.Update();
            }
        }

        private void CreateMenu()
        {
            LogMessage("Creating menu UI...");
            
            MenuUI = UniversalUI.RegisterUI(OutwardModsCommunicatorMenu.OMCM.GUID, () => { });
            
            MenuVisibilityManager.Instance.Initialize(MenuUI);
            
            _mainPanel = new MainPanel(MenuUI);
            
            LogMessage("Menu UI created successfully");
        }

        public void OnResourcesLoaded()
        {
            LogMessage("ResourcesPrefabManager.Load completed - Menu ready");
            CreateMenu();
            MenuVisibilityManager.Instance.SetVisible(true);
        }

        private void LogMessage(string message)
        {
            Log?.LogMessage($"[UIManager] {message}");
        }

        public class MainPanel : PanelBase
        {
            public MainPanel(UIBase owner) : base(owner) { }

            public override string Name => "Mods Communicator Menu";
            public override int MinWidth => 800;
            public override int MinHeight => 750;
            public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);
            public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);
            public override bool CanDragAndResize => true;

            protected override void OnClosePanelClicked()
            {
                MenuVisibilityManager.Instance.Toggle();
            }

            public override void SetDefaultSizeAndPosition()
            {
                Rect.localPosition = DefaultPosition;
                Rect.pivot = new Vector2(0.5f, 0.5f);
                Rect.anchorMin = DefaultAnchorMin;
                Rect.anchorMax = DefaultAnchorMax;

                LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
                EnsureValidPosition();
                EnsureValidSize();
            }

            protected override void ConstructPanelContent()
            {
                UI.MenuFactory.CreateEventPublishingPanel(ContentRoot);
            }
        }
    }
}
