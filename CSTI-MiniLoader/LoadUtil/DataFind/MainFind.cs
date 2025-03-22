using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CSTI_MiniLoader.LoadUtil.DataFind;

public static class MainFind
{
    public static IEnumerable<Object> Find(this UniqueIDScriptable idScriptable)
    {
        var traverse = Trav.Create(idScriptable);
        foreach (var field in traverse.Fields())
        {
            var tField = traverse.Field(field);
            if (tField.IsSubclassOf(typeof(Object)))
            {
                yield return tField.GetValue<Object>();
            }
        }
    }
}