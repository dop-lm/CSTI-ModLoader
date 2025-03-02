using HarmonyLib;
using UnityEngine;

namespace ModLoader.Patchers;

[HarmonyPatch]
public static class BpFixPatch
{
    [HarmonyPrefix, HarmonyPatch(typeof(MenuCardPreview), nameof(MenuCardPreview.ClickBlueprint))]
    public static bool MenuCardPreview_ClickBlueprint(MenuCardPreview __instance)
    {
        if (!__instance.AssociatedCard || !GameManager.Instance || !GraphicsManager.Instance)
            return false;
        if (__instance.AssociatedCard.GetUnlockConditions is { } unlockConditions &&
            !unlockConditions.IsUnlocked()) return true;
        if (GameManager.Instance.BlueprintModelStates.TryGetValue(__instance.AssociatedCard, out var modelState) &&
            modelState == BlueprintModelState.Locked)
        {
            var lastResearch = GraphicsManager.Instance.BlueprintModelsPopup.CurrentResearch;
            if (lastResearch)
            {
                foreach (var lockedBlueprintsPreview in GraphicsManager.Instance.BlueprintModelsPopup
                             .LockedBlueprintsPreviews)
                {
                    if (lockedBlueprintsPreview.AssociatedCard != lastResearch) continue;
                    lockedBlueprintsPreview.CardTitle.text = lastResearch.CardName;
                    break;
                }
            }

            __instance.CardTitle.text += "\n(研究中|Researching)";
            GraphicsManager.Instance.BlueprintModelsPopup.CurrentResearch = __instance.AssociatedCard;
            return false;
        }

        return true;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MenuCardPreview), nameof(MenuCardPreview.Setup))]
    public static void MenuCardPreview_Setup(MenuCardPreview __instance)
    {
        if (__instance.CardDesc && __instance.AssociatedCard)
        {
            __instance.CardDesc.text += $"\n点击锁定蓝图开始研究,共需{__instance.AssociatedCard.BlueprintUnlockSunsCost}天";
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(BlueprintModelsScreen), nameof(BlueprintModelsScreen.FinishBlueprintResearch))]
    public static void BlueprintModelsScreen_FinishBlueprintResearch(BlueprintModelsScreen __instance)
    {
        var currentResearch = __instance.CurrentResearch;
        GameManager.Instance.StartCoroutine(GameManager.Instance.AddCard(currentResearch, null!, true,
            GameManager.SpecialDrop.None, null!, null!,
            null, null, true, SpawningLiquid.Empty,
            new Vector2Int(GameManager.Instance.CurrentTickInfo.z, 0), null));
    }

    [HarmonyPostfix, HarmonyPatch(typeof(BlueprintModelsScreen), nameof(BlueprintModelsScreen.UpdateLockedBlueprints))]
    public static void BlueprintModelsScreen_UpdateLockedBlueprints(BlueprintModelsScreen __instance)
    {
        foreach (var menuCardPreview in __instance.LockedBlueprintsPreviews)
        {
            if (GameManager.Instance.BlueprintModelStates.ContainsKey(menuCardPreview.AssociatedCard) &&
                GameManager.Instance.BlueprintModelStates[menuCardPreview.AssociatedCard] ==
                BlueprintModelState.Locked &&
                menuCardPreview.AssociatedCard.GetUnlockConditions is { } unlockConditions &&
                !unlockConditions.IsUnlocked()) menuCardPreview.gameObject.SetActive(false);
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
    public static void GmInitPost()
    {
        GameManager.Instance.BlueprintPurchasing = true;
    }
}