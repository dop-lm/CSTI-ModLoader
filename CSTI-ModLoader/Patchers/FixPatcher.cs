using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ModLoader.Patchers
{
    public static class FixPatcher
    {
        public static void DoPatch(Harmony harmony)
        {
            MethodInfo methodInfo;
            try
            {
                methodInfo = AccessTools.Method(typeof(EncounterLogMessage), nameof(EncounterLogMessage.Duplicate),
                    new[] {typeof(EncounterLogMessage)});
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }

            harmony.Patch(methodInfo, new HarmonyMethod(typeof(FixPatcher), nameof(DuplicatePre)));
        }


        public static void DuplicatePre(ref EncounterLogMessage _From)
        {
            object tmp = _From;
            var (field, getter, setter) = typeof(EncounterLogMessage).FieldFromCache("AlternateLogTexts");
            if (field == null)
            {
                return;
            }

            if (getter(tmp) != null)
            {
                return;
            }

            setter(tmp, new LocalizedString[]{});
            _From = (EncounterLogMessage) tmp;
        }
    }
}