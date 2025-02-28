using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace ModLoader.Patchers;

[HarmonyPatch]
public static class EnvPatch
{
    private static bool IsNeedTravelToPreviousEnvSupportEnvironment(this CardData cardData)
    {
        return cardData.InstancedEnvironment ||
               cardData.TimeValues?.Any(objective => objective.ObjectiveName == "NeedTravelToPreviousEnvSupport") ==
               true;
    }

    private static bool IsNeedTravelToPreviousEnvSupportNotInstancedEnvironment(this CardData cardData)
    {
        return !cardData.InstancedEnvironment &&
               cardData.TimeValues?.Any(objective => objective.ObjectiveName == "NeedTravelToPreviousEnvSupport") ==
               true;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(EnvID), MethodType.Constructor, typeof(CardData), typeof(EnvID), typeof(int))]
    public static void EnvID_Ctor(ref EnvID __instance, CardData _Card, EnvID _FromEnv, int _WithIndex)
    {
        if (__instance.MainEnvCard && !_FromEnv.IsNull &&
            __instance.MainEnvCard.IsNeedTravelToPreviousEnvSupportNotInstancedEnvironment())
        {
            __instance.ParentEnvs = new List<ParentEnvironment>();
            if (_FromEnv.ParentEnvs is { Count: > 0 })
                __instance.ParentEnvs.AddRange(_FromEnv.ParentEnvs);
            __instance.ParentEnvs.Add(new ParentEnvironment(_FromEnv.EnvCard, _WithIndex));
        }
    }
}