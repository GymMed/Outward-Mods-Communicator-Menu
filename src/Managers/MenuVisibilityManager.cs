using System;
using UnityEngine.EventSystems;
using UniverseLib.UI;

namespace OutwardModsCommunicatorMenu.Managers
{
    public class MenuVisibilityManager
    {
        private static MenuVisibilityManager _instance;
        public static MenuVisibilityManager Instance => _instance ??= new MenuVisibilityManager();

        public bool IsVisible { get; private set; }
        public event Action<bool> OnVisibilityChanged;

        private UIBase _menuUI;
        
        private MenuVisibilityManager() { }

        public void Initialize(UIBase menuUI)
        {
            _menuUI = menuUI;
        }

        public void Toggle()
        {
            SetVisible(!IsVisible);
        }

        public void SetVisible(bool visible)
        {
            if (IsVisible == visible) return;
            
            IsVisible = visible;

            if (_menuUI?.RootObject != null)
            {
                _menuUI.RootObject.SetActive(visible);
            }

            if (!visible)
            {
                EventSystem.current?.SetSelectedGameObject(null);
            }
            
            OnVisibilityChanged?.Invoke(visible);
        }
    }
}
