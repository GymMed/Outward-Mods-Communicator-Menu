using UnityEngine;

namespace OutwardModsCommunicatorMenu.UI
{
    public static class MenuFactory
    {
        public static void CreateEventPublishingPanel(GameObject contentRoot)
        {
            EventPublishingPanelBuilder.Build(contentRoot);
        }
    }
}
