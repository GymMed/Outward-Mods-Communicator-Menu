using HarmonyLib;
using OutwardModsCommunicatorMenu.Managers;

namespace OutwardModsCommunicatorMenu.Patches
{
    [HarmonyPatch]
    public static class ResourcesPrefabManager_Load
    {
        [HarmonyPatch(typeof(ResourcesPrefabManager), nameof(ResourcesPrefabManager.Load))]
        public static void Postfix(ResourcesPrefabManager __instance)
        {
            UIManager.Instance.OnResourcesLoaded();
        }
    }
}
