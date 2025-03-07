using HarmonyLib;

namespace ModLoader.Patchers;

[HarmonyPatch]
public static class NpcPatch
{
    [HarmonyPatch(typeof(InGameCardBase), nameof(InGameCardBase.DropInInventory)), HarmonyPrefix]
    public static bool CanBeDragged(InGameCardBase _Card)
    {
        if (_Card.IsOwnedByNPC) return false;
        return true;
    }
}