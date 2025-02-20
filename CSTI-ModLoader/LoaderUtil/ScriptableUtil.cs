using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ModLoader.LoaderUtil;

public static class ScriptableUtil
{
    public static ScriptableObject CreateInstance(Type t, bool safe = true)
    {
        var scriptableObject = ScriptableObject.CreateInstance(t);
        if (safe)
        {
            var fields = AccessTools.GetDeclaredFields(t);
            foreach (var field in fields)
            {
                if (field.FieldType.IsArray)
                {
                    field.SetValue(scriptableObject, Array.CreateInstance(field.FieldType.GetElementType()!, 0));
                }

                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    field.SetValue(scriptableObject, Activator.CreateInstance(field.FieldType));
                }

                if (field.FieldType == typeof(string))
                {
                    field.SetValue(scriptableObject, "");
                }
            }
        }

        return scriptableObject;
    }
}