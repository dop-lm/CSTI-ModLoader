using System;
using System.Collections;
using System.Collections.Generic;
using CSTI_MiniLoader.LoadUtil;
using UnhollowerBaseLib;
using UnityEngine;

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
        var objType = obj.GetType();
        var genInfos = MainGen.GetOrGen(objType);

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
                    if (!genInfos.TryGetValue(fieldName, out var tuple)) continue;
                    var fieldWarpData = json[fieldName + "WarpData"];
                    MainGenTools.CommonSet((Il2CppObjectBase)obj, fieldName, fieldWarpData, (WarpType)keyData.Int);
                }
                else if (key.EndsWith("WarpData"))
                    continue;
                else
                {
                    if (keyData.IsObject)
                    {
                        var fieldName = key;
                        if (!genInfos.TryGetValue(fieldName, out var tuple)) continue;
                        if (tuple.Fld.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                            continue;
                        var subObj = MainGenTools.CommonGet((Il2CppObjectBase)obj, fieldName);
                        JsonCommonWarpper(subObj, keyData);
                        MainGenTools.CommonSetFld(obj, fieldName, subObj);
                    }
                    else if (keyData.IsArray)
                    {
                        var fieldName = key;
                        if (!genInfos.TryGetValue(fieldName, out var tuple)) continue;

                        for (var i = 0; i < keyData.Count; i++)
                        {
                            if (keyData[i].IsObject)
                            {
                                if (tuple.Fld.FieldType.IsGenericType &&
                                    tuple.Fld.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    // var ele_type = field.FieldType.GetGenericArguments().Single();
                                    if (tuple.Fld.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                        break;
                                    var list = (IList)MainGenTools.CommonGet(obj, fieldName)!;
                                    var ele = list!.get_Item(i);
                                    if (ele == null)
                                        continue;
                                    JsonCommonWarpper(ele, keyData[i]);
                                    list.set_Item(i, ele);
                                    MainGenTools.CommonSetFld(obj, fieldName, list);
                                }
                                else if (tuple.Fld.FieldType.IsArray)
                                {
                                    // var ele_type = field.FieldType.GetElementType();
                                    if (tuple.Fld.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                        break;
                                    var array = (IList)MainGenTools.CommonGet(obj, fieldName)!;
                                    object? ele = null;
                                    try
                                    {
                                        ele = array.get_Item(i);
                                    }
                                    catch (Exception e)
                                    {
                                        var id = "NullId";
                                        if (obj is UniqueIDScriptable uniqueIDScriptable)
                                        {
                                            id = uniqueIDScriptable.Uid();
                                        }
                                        else if (obj is ScriptableObject scriptableObject)
                                        {
                                            id = scriptableObject.name;
                                        }
                                    }

                                    if (ele == null)
                                        continue;
                                    JsonCommonWarpper(ele, keyData[i]);
                                    array.set_Item(i, ele);
                                    MainGenTools.CommonSetFld(obj, fieldName, array);
                                }
                            }
                        }
                    }
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }
        }
    }
}