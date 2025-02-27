using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ModLoader.Patchers;

[HarmonyPatch]
public static class NullFixPatch
{
    [HarmonyPrefix, HarmonyPatch(typeof(AmbienceImageEffect), nameof(AmbienceImageEffect.SetWeather))]
    public static bool AmbienceImageEffect_SetWeather(WeatherSet _Weather)
    {
        if (_Weather == null) return false;
        if (_Weather.EffectsToSpawn.Any(effect => effect == null))
            _Weather.EffectsToSpawn = _Weather.EffectsToSpawn.Where(effect => effect != null).ToArray();
        return true;
    }
}