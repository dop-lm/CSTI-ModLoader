using HarmonyLib;

namespace ModLoader.Patchers;

[HarmonyPatch]
public static class NpcPatch
{
    [HarmonyPatch(typeof(InGameCardBase), nameof(InGameCardBase.OnDrop)), HarmonyPrefix]
    public static bool InGameCardBase_OnDrop()
    {
        if (GameManager.DraggedCard && GameManager.DraggedCard.IsOwnedByNPC) return false;
        return true;
    }
}