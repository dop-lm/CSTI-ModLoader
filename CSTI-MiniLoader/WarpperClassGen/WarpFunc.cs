using System;
using System.Collections;
using System.Collections.Generic;
using CSTI_MiniLoader.LoadUtil;
using HarmonyLib;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;
using IList = Il2CppSystem.Collections.IList;
using Object = Il2CppSystem.Object;

namespace CSTI_MiniLoader.WarpperClassGen;

public static class WarpFunc
{
    public static void JsonCommonRefWarpper(object obj, KVProvider data, string fieldName,
        WarpType warpType = WarpType.REFERENCE)
    {
        MainGenTools.CommonSet((Il2CppObjectBase)obj, fieldName, data, warpType);
    }

    public static void JsonCommonWarpper(object? obj, KVProvider json)
    {
        if (!json.IsObject) return;
        if (obj == null) return;
        var traverse = Trav.Create(obj);

        foreach (var key in json.Keys)
        {
            try
            {
                var keyData = json[key];
                if (key.EndsWith("WarpType"))
                {
                    if (!keyData.IsInt || !json.ContainsKey(key.Substring(0, key.Length - 8) + "WarpData"))
                        continue;
                    var fieldName = key.Substring(0, key.Length - 8);
                    var fieldWarpData = json[fieldName + "WarpData"];
                    MainGenTools.CommonSet(obj, fieldName, fieldWarpData, (WarpType)keyData.Int);
                }
                else if (key.EndsWith("WarpData"))
                    continue;
                else
                {
                    if (keyData.IsObject)
                    {
                        var fieldName = key;
                        if (!traverse.Field(fieldName).FieldExists()) continue;
                        var subObj = traverse.Field(fieldName).GetValue();
                        JsonCommonWarpper(subObj, keyData);
                        traverse.Field(fieldName).SetValue(subObj);
                    }
                    else if (keyData.IsArray)
                    {
                        var fieldName = key;
                        if (!traverse.Field(fieldName).FieldExists()) continue;

                        for (var i = 0; i < keyData.Count; i++)
                        {
                            if (keyData[i].IsObject)
                            {
                                var o = MainGenTools.CommonGet(obj, fieldName)!;
                                object? ele = null;
                                if (o is Il2CppObjectBase il2CppObjectBase)
                                    ele = il2CppObjectBase.TryCast<IList>().get_Item(i);
                                else
                                    ele = ((System.Collections.IList)o)[i];
                                if (ele == null) continue;
                                JsonCommonWarpper(ele, keyData[i]);
                                if (o is Il2CppObjectBase il2CppObjectBase0)
                                    il2CppObjectBase0.TryCast<IList>().set_Item(i, (Object)ele);
                                else
                                    ((System.Collections.IList)o)[i] = ele;
                                MainGenTools.CommonSetFld(obj, fieldName, o);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
            }
        }
    }
}