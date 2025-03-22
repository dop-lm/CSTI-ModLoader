using System.Collections.Generic;
using UnityEngine;

namespace CSTI_MiniLoader.LoadUtil.DataFind;

public static class MainFind
{
    // TODO:在这卡死，原因未知
    public static IEnumerable<Object> Find(this UniqueIDScriptable idScriptable)
    {
        var traverse = Trav.Create(idScriptable);
        foreach (var field in traverse.Fields())
        {
            var tField = traverse.Field(field);
            if (tField.IsSubclassOf(typeof(Object)))
            {
                yield return tField.GetValue().SafeCast<Object>();
            }
        }
    }
}