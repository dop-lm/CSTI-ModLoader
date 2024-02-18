using System;
using System.Linq;
using CSTI_MiniLoader.LoadUtil;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using Array = Il2CppSystem.Array;
using Object = Il2CppSystem.Object;

namespace CSTI_MiniLoader.WarpperClassGen;

public static class MainGenTools
{
    public static object? CommonGet(object baseObj, string fld)
    {
        if (baseObj is not Il2CppObjectBase)
        {
            return Traverse.Create(baseObj).Field(fld).GetValue();
        }

        var valueTuples = MainGen.GetOrGen(baseObj.GetType());
        if (!valueTuples.TryGetValue(fld, out var tuple)) return null;
        if (tuple.isValueType)
        {
            return AccessTools.Method(typeof(MainGenTools), nameof(CommonGetVal), null, [tuple.fld.FieldType])
                .Invoke(null, [baseObj, fld]);
        }
        else
        {
            return AccessTools.Method(typeof(MainGenTools), nameof(CommonGetCls), null, [tuple.fld.FieldType])
                .Invoke(null, [baseObj, fld]);
        }
    }

    public static T CommonGetVal<T>(Il2CppObjectBase baseObj, string fld)
        where T : struct
    {
        var valueTuples = MainGen.GetOrGen(baseObj.GetType());
        if (!valueTuples.TryGetValue(fld, out var tuple)) return default;
        var objHandle = IL2CPP.Il2CppObjectBaseToPtrNotNull(baseObj);
        unsafe
        {
            return *(T*)(objHandle + tuple.fOffset);
        }
    }

    public static T? CommonGetCls<T>(Il2CppObjectBase baseObj, string fld)
        where T : Il2CppObjectBase
    {
        var valueTuples = MainGen.GetOrGen(baseObj.GetType());
        if (!valueTuples.TryGetValue(fld, out var tuple)) return null;
        var objHandle = IL2CPP.Il2CppObjectBaseToPtrNotNull(baseObj);
        unsafe
        {
            var intPtr = *(IntPtr*)(objHandle + tuple.fOffset);
            return intPtr == IntPtr.Zero
                ? null
                : (T)AccessTools.DeclaredConstructor(typeof(T), [typeof(IntPtr)]).Invoke([intPtr]);
        }
    }

    public static void CommonSetFld(object baseObj, string fld, object? data)
    {
        if (baseObj is not Il2CppObjectBase)
        {
            Traverse.Create(baseObj).Field(fld).SetValue(data);
            return;
        }

        var valueTuples = MainGen.GetOrGen(baseObj.GetType());
        if (!valueTuples.TryGetValue(fld, out var tuple)) return;
        if (tuple.isValueType)
        {
            AccessTools.DeclaredMethod(typeof(MainGenTools), nameof(CommonSetFldVal), null, [tuple.fld.FieldType])
                .Invoke(null, [baseObj, fld, data]);
        }
        else
        {
            AccessTools.DeclaredMethod(typeof(MainGenTools), nameof(CommonSetFldCls), null, [tuple.fld.FieldType])
                .Invoke(null, [baseObj, fld, data]);
        }
    }

    public static void CommonSetFldVal<T>(Il2CppObjectBase baseObj, string fld, T data)
        where T : struct
    {
        var valueTuples = MainGen.GetOrGen(baseObj.GetType());
        if (!valueTuples.TryGetValue(fld, out var tuple)) return;
        var objHandle = IL2CPP.Il2CppObjectBaseToPtrNotNull(baseObj);
        unsafe
        {
            *(T*)(objHandle + tuple.fOffset) = data;
        }
    }

    public static void CommonSetFldCls<T>(Il2CppObjectBase baseObj, string fld, T data)
        where T : Il2CppObjectBase
    {
        var valueTuples = MainGen.GetOrGen(baseObj.GetType());
        if (!valueTuples.TryGetValue(fld, out var tuple)) return;
        var objHandle = IL2CPP.Il2CppObjectBaseToPtrNotNull(baseObj);
        IL2CPP.il2cpp_gc_wbarrier_set_field(objHandle, objHandle + tuple.fOffset,
            IL2CPP.Il2CppObjectBaseToPtr(data));
    }

    public static void CommonSet(Il2CppObjectBase baseObj, string fld, KVProvider warpData, WarpType warpType)
    {
        var valueTuples = MainGen.GetOrGen(baseObj.GetType());
        if (!valueTuples.TryGetValue(fld, out var tuple)) return;
        if (warpData is { IsArray: true } &&
            tuple.fld.FieldType.GetGenericTypeDefinition().BaseType == typeof(Il2CppArrayBase<>))
        {
            if (warpData.Count == 0 || warpData[0].IsString)
            {
                var methodInfo =
                    AccessTools.Method(typeof(MainGenTools), nameof(SetArrByWarpper), null,
                        [tuple.fld.FieldType.GetGenericArguments().First()]);
                methodInfo.Invoke(null, [baseObj, fld, warpData, warpType]);
            }
            else if (warpData.Count > 0 && warpData[0].IsObject)
            {
                var methodInfo =
                    AccessTools.Method(typeof(MainGenTools), nameof(SetArrNoWarpper), null,
                        [tuple.fld.FieldType.GetGenericArguments().First()]);
                methodInfo.Invoke(null, [baseObj, fld, warpData, warpType]);
            }
        }
        else if (warpData is { IsArray: true } &&
                 tuple.fld.FieldType.GetGenericTypeDefinition() == typeof(List<>))
        {
            if (warpData.Count == 0 || warpData[0].IsString)
            {
                var methodInfo =
                    AccessTools.Method(typeof(MainGenTools), nameof(SetLiByWarpper), null,
                        [tuple.fld.FieldType.GetGenericArguments().First()]);
                methodInfo.Invoke(null, [baseObj, fld, warpData, warpType]);
            }
            else if (warpData.Count > 0 && warpData[0].IsObject)
            {
                var methodInfo =
                    AccessTools.Method(typeof(MainGenTools), nameof(SetLiNoWarpper), null,
                        [tuple.fld.FieldType.GetGenericArguments().First()]);
                methodInfo.Invoke(null, [baseObj, fld, warpData, warpType]);
            }
        }
        else if (warpData.IsString)
        {
            var methodInfo =
                AccessTools.Method(typeof(MainGenTools), nameof(SetByWarpper), null, [tuple.fld.FieldType]);
            methodInfo.Invoke(null, [baseObj, fld, warpData, warpType]);
        }
    }

    public static void SetByWarpper<T>(Il2CppObjectBase baseObj, string fld, KVProvider warpData, WarpType warpType)
    {
        if (AllItemDictionary.TryGetValue(typeof(T), out var typedItems) &&
            typedItems.TryGetValue(warpData.ToString(), out var typedItem) && typedItem is T item)
        {
            var objHandle = IL2CPP.Il2CppObjectBaseToPtrNotNull(baseObj);
            var valueTuples = MainGen.GetOrGen(typeof(T));
            if (!valueTuples.TryGetValue(fld, out var tuple)) return;
            if (tuple.isValueType)
            {
                unsafe
                {
                    *(T*)(objHandle + tuple.fOffset) = item;
                }
            }
            else
            {
                IL2CPP.il2cpp_gc_wbarrier_set_field(objHandle, objHandle + tuple.fOffset,
                    IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)item));
            }
        }
    }

    public static void SetLiByWarpper<T>(Il2CppObjectBase baseObj, string fld, KVProvider warpData, WarpType warpType)
    {
        if (AllItemDictionary.TryGetValue(typeof(T), out var typedItems))
        {
            var objHandle = IL2CPP.Il2CppObjectBaseToPtrNotNull(baseObj);
            var valueTuples = MainGen.GetOrGen(typeof(T));
            if (!valueTuples.TryGetValue(fld, out var tuple)) return;
            unsafe
            {
                var li = *(IntPtr*)(objHandle + tuple.fOffset);
                var list = li != IntPtr.Zero ? new List<T>(li) : new List<T>();
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
    }

    public static void SetLiNoWarpper<T>(Il2CppObjectBase baseObj, string fld, KVProvider warpData, WarpType warpType)
    {
        var objHandle = IL2CPP.Il2CppObjectBaseToPtrNotNull(baseObj);
        var valueTuples = MainGen.GetOrGen(typeof(T));
        if (!valueTuples.TryGetValue(fld, out var tuple)) return;
        unsafe
        {
            var li = *(IntPtr*)(objHandle + tuple.fOffset);
            var list = li != IntPtr.Zero ? new List<T>(li) : new List<T>();
            if (warpType == WarpType.MODIFY) list.Clear();
            for (var i = 0; i < warpData.Count; i++)
            {
                var scriptableObject = typeof(T).IsSubclassOf(typeof(ScriptableObject))
                    ? (T)(object)ScriptableObject.CreateInstance(Il2CppType.Of<T>())
                    : AccessTools.CreateInstance<T>();
                WarpFunc.JsonCommonWarpper(scriptableObject, warpData[i]);
                list.Add(scriptableObject);
            }
        }
    }

    public static void SetArrByWarpper<T>(Il2CppObjectBase baseObj, string fld, KVProvider warpData, WarpType warpType)
        where T : Il2CppObjectBase
    {
        if (AllItemDictionary.TryGetValue(typeof(T), out var typedItems))
        {
            var objHandle = IL2CPP.Il2CppObjectBaseToPtrNotNull(baseObj);
            var valueTuples = MainGen.GetOrGen(typeof(T));
            if (!valueTuples.TryGetValue(fld, out var tuple)) return;
            unsafe
            {
                var arr = *(IntPtr*)(objHandle + tuple.fOffset);
                var cacheTLi = warpType == WarpType.MODIFY || arr == IntPtr.Zero
                    ? new System.Collections.Generic.List<T>()
                    : new Il2CppReferenceArray<T>(arr).ToList();
                for (var i = 0; i < warpData.Count; i++)
                {
                    if (typedItems.TryGetValue(warpData[i].ToString(), out var typedItem) && typedItem is T item)
                    {
                        cacheTLi.Add(item);
                    }
                }

                var newArr = Array.CreateInstance(Il2CppType.Of<T>(), cacheTLi.Count);
                for (var i = 0; i < cacheTLi.Count; i++)
                {
                    newArr.SetValue((Object)(Il2CppObjectBase)cacheTLi[i], i);
                }

                IL2CPP.il2cpp_gc_wbarrier_set_field(objHandle, objHandle + tuple.fOffset,
                    IL2CPP.Il2CppObjectBaseToPtr(newArr));
            }
        }
    }

    public static void SetArrNoWarpper<T>(Il2CppObjectBase baseObj, string fld, KVProvider warpData, WarpType warpType)
        where T : Il2CppObjectBase
    {
        var objHandle = IL2CPP.Il2CppObjectBaseToPtrNotNull(baseObj);
        var valueTuples = MainGen.GetOrGen(typeof(T));
        if (!valueTuples.TryGetValue(fld, out var tuple)) return;
        unsafe
        {
            var arr = *(IntPtr*)(objHandle + tuple.fOffset);
            var cacheTLi = warpType == WarpType.MODIFY || arr == IntPtr.Zero
                ? new System.Collections.Generic.List<T>()
                : new Il2CppReferenceArray<T>(arr).ToList();
            for (var i = 0; i < warpData.Count; i++)
            {
                var scriptableObject = typeof(T).IsSubclassOf(typeof(ScriptableObject))
                    ? (T)(object)ScriptableObject.CreateInstance(Il2CppType.Of<T>())
                    : AccessTools.CreateInstance<T>();
                WarpFunc.JsonCommonWarpper(scriptableObject, warpData[i]);
                cacheTLi.Add(scriptableObject);
            }

            var newArr = Array.CreateInstance(Il2CppType.Of<T>(), cacheTLi.Count);
            for (var i = 0; i < cacheTLi.Count; i++)
            {
                newArr.SetValue((Object)(Il2CppObjectBase)cacheTLi[i], i);
            }

            IL2CPP.il2cpp_gc_wbarrier_set_field(objHandle, objHandle + tuple.fOffset,
                IL2CPP.Il2CppObjectBaseToPtr(newArr));
        }
    }
}