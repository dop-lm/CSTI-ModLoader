using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using HarmonyLib;
using Unity.Collections.LowLevel.Unsafe;

namespace ModLoader;

public static class WarpHelper
{
    public static readonly Dictionary<Type, Dictionary<string, FieldInfo>> FieldInfoCache = new();

    public static readonly Dictionary<Type, Dictionary<string, Func<object, object>>>
        FieldGetterDynamicMethodCache = new();

    public static readonly Dictionary<Type, Dictionary<string, Action<object, object>>>
        FieldSetterDynamicMethodCache = new();

    public static readonly Dictionary<Type, Func<object>> ClassConstructorCache = new();

    public static Func<object> ConstructorFromCache(this Type type)
    {
        if (ClassConstructorCache.TryGetValue(type, out var cache))
        {
            return cache;
        }

        if (type.IsValueType)
        {
            ClassConstructorCache[type] = CreateConstructor(type, null);
            return ClassConstructorCache[type];
        }

        ClassConstructorCache[type] = CreateConstructor(type,
            AccessTools.FirstConstructor(type, info => info.GetParameters().Length == 0));
        return ClassConstructorCache[type];
    }

    public static Func<object> CreateConstructor(Type type, ConstructorInfo constructorInfo)
    {
        var dynamicMethod = new DynamicMethod("createInstance", typeof(object), Type.EmptyTypes);
        var ilGenerator = dynamicMethod.GetILGenerator();
        if (!type.IsValueType)
        {
            ilGenerator.Emit(OpCodes.Newobj, constructorInfo);
            ilGenerator.Emit(OpCodes.Ret);
        }
        else
        {
            var declareLocal = ilGenerator.DeclareLocal(type);
            ilGenerator.Emit(OpCodes.Ldloc_S, declareLocal);
            ilGenerator.Emit(OpCodes.Box, type);
            ilGenerator.Emit(OpCodes.Ret);
        }

        return dynamicMethod.CreateDelegate(typeof(Func<object>)) as Func<object>;
    }

    public static (FieldInfo field, Func<object, object> getter, Action<object, object> setter) FieldFromCache(
        this Type type, string field_name, bool getter_use = true, bool setter_use = true)
    {
        FieldInfo fieldInfo;
        Func<object, object> getter = null;
        Action<object, object> setter = null;
        if (FieldInfoCache.TryGetValue(type, out var fieldInfos))
        {
            if (fieldInfos.TryGetValue(field_name, out var info))
            {
                fieldInfo = info;
            }
            else
            {
                fieldInfo = AccessTools.Field(type, field_name);
                fieldInfos[field_name] = fieldInfo;
            }
        }
        else
        {
            fieldInfo = AccessTools.Field(type, field_name);
            FieldInfoCache[type] = new Dictionary<string, FieldInfo> { { field_name, fieldInfo } };
        }

        if (fieldInfo != null && getter_use)
        {
            if (FieldGetterDynamicMethodCache.TryGetValue(type, out var delegates))
            {
                if (delegates.TryGetValue(field_name, out var getFunc))
                {
                    getter = getFunc;
                }
                else
                {
                    getter = GenGetter(fieldInfo, type);
                    delegates[field_name] = getter;
                }
            }
            else
            {
                getter = GenGetter(fieldInfo, type);
                FieldGetterDynamicMethodCache[type] = new Dictionary<string, Func<object, object>>
                    { { field_name, getter } };
            }
        }

        if (fieldInfo != null && setter_use)
        {
            if (FieldSetterDynamicMethodCache.TryGetValue(type, out var delegates))
            {
                if (delegates.TryGetValue(field_name, out var setFunc))
                {
                    setter = setFunc;
                }
                else
                {
                    setter = GenSetter(fieldInfo, type);
                    delegates[field_name] = setter;
                }
            }
            else
            {
                setter = GenSetter(fieldInfo, type);
                FieldSetterDynamicMethodCache[type] = new Dictionary<string, Action<object, object>>
                    { { field_name, setter } };
            }
        }

        return (fieldInfo, getter, setter);
    }

    private static readonly Type ObjType = typeof(object);

    public static Func<object, object> GenGetter(FieldInfo fieldInfo, Type type)
    {
        var fieldInfoFieldType = fieldInfo.FieldType;
        var typeIsValueType = type.IsValueType;
        var fieldTypeIsValueType = fieldInfoFieldType.IsValueType;
        if (!typeIsValueType && !fieldTypeIsValueType)
        {
            var accessHelper = AccessHelper.ByOffset(UnsafeUtility.GetFieldOffset(fieldInfo));
            return accessHelper.Get;
        }

        var dynamicMethod = new DynamicMethod("simple_getter", ObjType, new[] { ObjType }, true);
        var ilGenerator = dynamicMethod.GetILGenerator();
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(typeIsValueType ? OpCodes.Unbox : OpCodes.Castclass, type);
        ilGenerator.Emit(OpCodes.Ldfld, fieldInfo);
        if (fieldTypeIsValueType)
        {
            ilGenerator.Emit(OpCodes.Box, fieldInfoFieldType);
        }

        ilGenerator.Emit(OpCodes.Ret);
        var getter = dynamicMethod.CreateDelegate(typeof(Func<object, object>)) as Func<object, object>;
        return getter;
    }

    public static Action<object, object> GenSetter(FieldInfo fieldInfo, Type type)
    {
        var fieldInfoFieldType = fieldInfo.FieldType;
        var isValueType = fieldInfoFieldType.IsValueType;
        var typeIsValueType = type.IsValueType;
        if (!typeIsValueType && !isValueType)
        {
            var accessHelper = AccessHelper.ByOffset(UnsafeUtility.GetFieldOffset(fieldInfo));
            return accessHelper.Set;
        }

        var dynamicMethod = new DynamicMethod("simple_setter", typeof(void), new[] { ObjType, ObjType }, true);
        var ilGenerator = dynamicMethod.GetILGenerator();
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(typeIsValueType ? OpCodes.Unbox : OpCodes.Castclass, type);
        ilGenerator.Emit(OpCodes.Ldarg_1);
        ilGenerator.Emit(isValueType ? OpCodes.Unbox_Any : OpCodes.Castclass,
            fieldInfoFieldType);
        ilGenerator.Emit(OpCodes.Stfld, fieldInfo);
        ilGenerator.Emit(OpCodes.Ret);
        var setter = dynamicMethod.CreateDelegate(typeof(Action<object, object>)) as Action<object, object>;
        return setter;
    }

    public unsafe class AccessHelper
    {
        private static readonly Dictionary<int, WeakReference<AccessHelper>> UsedAccessHelper = new();
        private readonly int Offset;
        private static readonly UnsafeTool Unsafe = new();

        public static AccessHelper ByOffset(int offset)
        {
            if (UsedAccessHelper.TryGetValue(offset, out var accessReference))
            {
                if (accessReference.TryGetTarget(out var accessHelper))
                {
                    return accessHelper;
                }

                accessHelper = new AccessHelper(offset);
                accessReference.SetTarget(accessHelper);
                return accessHelper;
            }

            var helper = new AccessHelper(offset);
            UsedAccessHelper[offset] = new WeakReference<AccessHelper>(helper);
            return helper;
        }

        private AccessHelper(int offset)
        {
            Offset = offset;
            UsedAccessHelper[offset] = new WeakReference<AccessHelper>(this);
        }

        public object Get(object o)
        {
            var ptr = UnsafeUtility.PinGCObjectAndGetAddress(o, out var gcHandle);
            var obj = Unsafe.ToObj(*(void**)((IntPtr)ptr + Offset));
            UnsafeUtility.ReleaseGCObject(gcHandle);
            return obj;
        }

        public void Set(object o, object val)
        {
            var ptr = UnsafeUtility.PinGCObjectAndGetAddress(o, out var gcHandle);
            var ptrVal = UnsafeUtility.PinGCObjectAndGetAddress(val, out var gcHandle1);
            *(void**)((IntPtr)ptr + Offset) = ptrVal;
            UnsafeUtility.ReleaseGCObject(gcHandle);
            UnsafeUtility.ReleaseGCObject(gcHandle1);
        }

        [StructLayout(LayoutKind.Explicit)]
        private class UnsafeTool
        {
            public delegate void* Obj2PtrFunc(object o);

            public delegate object Ptr2ObjFunc(void* o);

            // ReSharper disable once NotAccessedField.Local
            [FieldOffset(0)] private Func<object, object> __accessBackend;

#pragma warning disable CS0649
            [FieldOffset(0)] public Obj2PtrFunc ToPtr;
            [FieldOffset(0)] public Ptr2ObjFunc ToObj;
#pragma warning restore CS0649

            public UnsafeTool()
            {
                __accessBackend = __transform;
            }

            private object __transform(object o)
            {
                return o;
            }
        }
    }

    public static void Deconstruct<TKey, TVal>(this KeyValuePair<TKey, TVal> keyValuePair, out TKey key,
        out TVal val)
    {
        key = keyValuePair.Key;
        val = keyValuePair.Value;
    }
}