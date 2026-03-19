using HarmonyLib;
using OutwardModsCommunicatorMenu.Managers;
using System;
using UnityEngine;

namespace OutwardModsCommunicatorMenu.Patches
{
    [HarmonyPatch]
    public static class ResourcesPrefabManager_Load
    {
        [HarmonyPatch(typeof(ResourcesPrefabManager), nameof(ResourcesPrefabManager.Load))]
        public static void Postfix(ResourcesPrefabManager __instance)
        {
            try
            {
                UIManager.Instance.OnResourcesLoaded();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OMM] Error in ResourcesPrefabManager_Load patch: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
