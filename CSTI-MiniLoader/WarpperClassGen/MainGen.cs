using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnhollowerBaseLib;

namespace CSTI_MiniLoader.WarpperClassGen;

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
    public class Warp
    {
        public FieldInfo Fld;
        public IntPtr FPtr;
        public int FOffset;
        public bool IsValueType;

        public Warp(FieldInfo fld, IntPtr fPtr, int fOffset, bool isValueType)
        {
            this.Fld = fld;
            this.FPtr = fPtr;
            this.FOffset = fOffset;
            this.IsValueType = isValueType;
        }
    }

    public static readonly
        Dictionary<Type, Dictionary<string, Warp>>
        WarpperTypes = new();

    static MainGen()
    {
    }

    public static Dictionary<string, Warp> GetOrGen(Type type)
    {
        if (WarpperTypes.TryGetValue(type, out var warpperType)) return warpperType;
        var warpper = new Dictionary<string, Warp>();
        WarpperTypes[type]= warpper;

        foreach (var field in AccessTools.GetDeclaredFields(type))
        {
            if (!field.IsStatic || !field.Name.StartsWith("NativeFieldInfoPtr")) continue;
            var fPtr = (IntPtr)field.GetValue(null);
            warpper[field.Name.Substring("NativeFieldInfoPtr_".Length)]=
                new Warp(field, fPtr, (int)IL2CPP.il2cpp_field_get_offset(fPtr), field.FieldType.IsValueType);
        }

        return warpper;
    }
}