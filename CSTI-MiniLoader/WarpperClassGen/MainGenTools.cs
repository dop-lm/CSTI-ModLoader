using System;
using System.Collections;
using System.Collections.Generic;
using CSTI_MiniLoader.LoadUtil;
using HarmonyLib;
using UnityEngine;

namespace CSTI_MiniLoader.WarpperClassGen;

public static class MainGenTools
{
    public static object? CommonGet(object baseObj, string fld)
    {
        return Traverse.Create(baseObj).Field(fld).GetValue();
    }

    public static void CommonSetFld(object baseObj, string fld, object? data)
    {
        Traverse.Create(baseObj).Field(fld).SetValue(data);
    }

    public static void CommonSet(object baseObj, string fld, KVProvider warpData, WarpType warpType)
    {
        var valueTuples = MainGen.GetOrGen(baseObj.GetType());
        if (!valueTuples.TryGetValue(fld, out var tuple)) return;
        if (warpData is { IsArray: true } &&
            tuple.Fld.FieldType.IsArray)
        {
            if (warpData.Count == 0 || warpData[0].IsString)
            {
                var methodInfo =
                    AccessTools.Method(typeof(MainGenTools), nameof(SetArrByWarpper), null,
                        [tuple.Fld.FieldType.GetGenericArguments().First()]);
                methodInfo.Invoke(null, [baseObj, fld, warpData, warpType]);
            }
            else if (warpData.Count > 0 && warpData[0].IsObject)
            {
                var methodInfo =
                    AccessTools.Method(typeof(MainGenTools), nameof(SetArrNoWarpper), null,
                        [tuple.Fld.FieldType.GetGenericArguments().First()]);
                methodInfo.Invoke(null, [baseObj, fld, warpData, warpType]);
            }
        }
        else if (warpData is { IsArray: true } &&
                 tuple.Fld.FieldType.GetGenericTypeDefinition() == typeof(List<>))
        {
            if (warpData.Count == 0 || warpData[0].IsString)
            {
                var methodInfo =
                    AccessTools.Method(typeof(MainGenTools), nameof(SetLiByWarpper), null,
                        [tuple.Fld.FieldType.GetGenericArguments().First()]);
                methodInfo.Invoke(null, [baseObj, fld, warpData, warpType]);
            }
            else if (warpData.Count > 0 && warpData[0].IsObject)
            {
                var methodInfo =
                    AccessTools.Method(typeof(MainGenTools), nameof(SetLiNoWarpper), null,
                        [tuple.Fld.FieldType.GetGenericArguments().First()]);
                methodInfo.Invoke(null, [baseObj, fld, warpData, warpType]);
            }
        }
        else if (warpData.IsString)
        {
            var methodInfo =
                AccessTools.Method(typeof(MainGenTools), nameof(SetByWarpper), null, [tuple.Fld.FieldType]);
            methodInfo.Invoke(null, [baseObj, fld, warpData, warpType]);
        }
    }

    public static void SetByWarpper<T>(object baseObj, string fld, KVProvider warpData, WarpType warpType)
    {
        if (AllItemDictionary.TryGetValue(typeof(T), out var typedItems) &&
            typedItems.TryGetValue(warpData.ToString(), out var typedItem) && typedItem is T item)
        {
            var traverse = Traverse.Create(baseObj);
            var tfld = traverse.Field(fld);
            tfld.SetValue(item);
        }
    }

    public static void SetLiByWarpper<T>(object baseObj, string fld, KVProvider warpData, WarpType warpType)
    {
        if (AllItemDictionary.TryGetValue(typeof(T), out var typedItems))
        {
            var traverse = Traverse.Create(baseObj);
            var li = traverse.Field<IList>(fld);
            var list = li.Value;
            if (warpType == WarpType.MODIFY) list.Clear();
            for (var i = 0; i < warpData.Count; i++)
            {
                if (typedItems.TryGetValue(warpData[i].ToString(), out var typedItem) && typedItem is T item)
                {
                    list.Add(item);
                }
            }
        }
    }

    public static void SetLiNoWarpper<T>(object baseObj, string fld, KVProvider warpData, WarpType warpType)
    {
        var traverse = Traverse.Create(baseObj);
        var li = traverse.Field<IList>(fld);
        var list = li.Value;
        if (warpType == WarpType.MODIFY) list.Clear();
        for (var i = 0; i < warpData.Count; i++)
        {
            var scriptableObject = typeof(T).IsSubclassOf(typeof(ScriptableObject))
                ? (T)(object)ScriptableObject.CreateInstance(typeof(T))
                : AccessTools.CreateInstance<T>();
            WarpFunc.JsonCommonWarpper(scriptableObject, warpData[i]);
            list.Add(scriptableObject);
        }
    }

    public static void SetArrByWarpper<T>(object baseObj, string fld, KVProvider warpData, WarpType warpType)
    {
        if (AllItemDictionary.TryGetValue(typeof(T), out var typedItems))
        {
            var traverse = Traverse.Create(baseObj);
            var arr = traverse.Field<IList>(fld);
            var cacheTLi = arr.Value.ToList();
            
            for (var i = 0; i < warpData.Count; i++)
            {
                if (typedItems.TryGetValue(warpData[i].ToString(), out var typedItem) && typedItem is T item)
                {
                    cacheTLi.Add(item);
                }
            }

            var newArr = new T[cacheTLi.Count];
            for (var i = 0; i < cacheTLi.Count; i++)
            {
                newArr.SetValue(cacheTLi.get_Item(i), i);
            }

            arr.Value = newArr;
        }
    }

    public static void SetArrNoWarpper<T>(object baseObj, string fld, KVProvider warpData, WarpType warpType)
    {
        var traverse = Traverse.Create(baseObj);
        var arr = traverse.Field<IList>(fld);
        var cacheTLi = arr.Value.ToList();
        
        for (var i = 0; i < warpData.Count; i++)
        {
            var scriptableObject = typeof(T).IsSubclassOf(typeof(ScriptableObject))
                ? (T)(object)ScriptableObject.CreateInstance(typeof(T))
                : AccessTools.CreateInstance<T>();
            WarpFunc.JsonCommonWarpper(scriptableObject, warpData[i]);
            cacheTLi.Add(scriptableObject);
        }

        var newArr = new T[cacheTLi.Count];
        for (var i = 0; i < cacheTLi.Count; i++)
        {
            newArr.SetValue(cacheTLi.get_Item(i), i);
        }

        arr.Value = newArr;
    }
}