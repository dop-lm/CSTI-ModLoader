using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CSTI_MiniLoader.LoadUtil.DataFind;

public static class MainFind
{
    public static IEnumerable<Object> Find(this UniqueIDScriptable idScriptable)
    {
        var traverse = Traverse.Create(idScriptable);
        foreach (var field in traverse.Fields())
        {
            var tField = traverse.Field(field);
            if (tField.GetValueType().IsSubclassOf(typeof(Object)))
            {
                yield return (Object)tField.GetValue();
            }
        }
    }
}