using HarmonyLib;
using UnityEngine;

namespace ModLoader.Patchers;

[HarmonyPatch]
public static class BpFixPatch
{
    [HarmonyPostfix, HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
    public static void GmInitPost()
    {
        GameManager.Instance.BlueprintPurchasing = true;
    }
}