using System.Collections;
using System.Collections.Generic;
using Cpp2IL.Core;
using UnityEngine;

namespace CSTI_MiniLoader.LoadUtil;

public static class Utils
{
    public static Transform Find(this Transform parent, string name)
    {
        foreach (var t in parent)
        {
            if (t is Transform tr && tr.name == name)
            {
                return tr;
            }
        }

        return parent;
    }

    public static List<T> ToList<T>(this IEnumerable<T> array)
    {
        return [..array];
    }

    public static List<object> ToList(this IEnumerable array)
    {
        return [..array];
    }

    public static T First<T>(this IEnumerable<T> array)
    {
        foreach (var t in array)
        {
            return t;
        }

        return default;
    }

    public static IEnumerable<T> Intersect<T>(this List<T> ea, IEnumerable<T> eb)
    {
        foreach (var t in eb)
        {
            if (ea.Contains(t))
            {
                yield return t;
            }
        }
    }

    public static string Uid(this UniqueIDScriptable o)
    {
        var saveID = UniqueIDScriptable.SaveID(o);
        var i = saveID.IndexOf("(");
        return saveID.Substring(0, i);
    }
}