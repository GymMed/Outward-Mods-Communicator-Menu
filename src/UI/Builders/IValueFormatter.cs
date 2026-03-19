using System;
using UnityEngine;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public interface IValueFormatter
    {
        bool CanFormat(object value);
        void CreateDisplay(GameObject parent, string name, object value, Type displayType);
    }
}
