using System;
using UnityEngine;
using UniverseLib.UI;
using UniverseLib.UI.Widgets;

namespace OutwardModsCommunicatorMenu.UI.Components
{
    public static class ScrollViewHelper
    {
        public static (GameObject root, GameObject content) CreateScrollView(
            GameObject parent,
            string name,
            Vector2 size,
            Color? bgColor = null)
        {
            GameObject scrollViewRoot = UIFactory.CreateScrollView(
                parent,
                name,
                out GameObject content,
                out AutoSliderScrollbar autoScrollbar,
                bgColor ?? new Color(0.2f, 0.2f, 0.25f, 0.95f)
            );

            UIFactory.SetLayoutElement(scrollViewRoot, 
                minWidth: (int)size.x, 
                flexibleWidth: 9999,
                minHeight: (int)size.y, 
                flexibleHeight: 9999);

            return (scrollViewRoot, content);
        }

        public static (GameObject root, GameObject content) CreateScrollView(
            GameObject parent,
            string name,
            int minWidth,
            int minHeight,
            Color? bgColor = null)
        {
            return CreateScrollView(parent, name, new Vector2(minWidth, minHeight), bgColor);
        }
    }
}
