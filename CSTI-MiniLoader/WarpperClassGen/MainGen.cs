using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnhollowerBaseLib;

namespace CSTI_MiniLoader.WarpperClassGen;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum WarpType
{
    NONE,
    COPY,
    CUSTOM,
    REFERENCE,
    ADD,
    MODIFY,
    ADD_REFERENCE
}

public static class MainGen
{
    public static readonly
        Dictionary<Type, Dictionary<string, (FieldInfo fld, IntPtr fPtr, int fOffset, bool isValueType)>>
        WarpperTypes = new();

    static MainGen()
    {
    }

    public static Dictionary<string, (FieldInfo fld, IntPtr fPtr, int fOffset, bool isValueType)> GetOrGen(Type type)
    {
        if (WarpperTypes.TryGetValue(type, out var warpperType)) return warpperType;
        var warpper = new Dictionary<string, (FieldInfo fld, IntPtr fPtr, int fOffset, bool isValueType)>();
        WarpperTypes[type] = warpper;

        foreach (var field in AccessTools.GetDeclaredFields(type))
        {
            if (!field.IsStatic || !field.Name.StartsWith("NativeFieldInfoPtr")) continue;
            var fPtr = (IntPtr)field.GetValue(null);
            warpper[field.Name.Substring("NativeFieldInfoPtr_".Length)] =
                (field, fPtr, (int)IL2CPP.il2cpp_field_get_offset(fPtr), field.FieldType.IsValueType);
        }

        return warpper;
    }
}