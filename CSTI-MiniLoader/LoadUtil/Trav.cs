using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppSystem.Reflection;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using Object = Il2CppSystem.Object;

namespace CSTI_MiniLoader.LoadUtil;

public class Trav
{
    private bool _isIl2CPP;
    private object _o;
    private Il2CppSystem.Type _il2CPPClass;
    private Traverse _traverse;
    private MemberInfo? _il2CPPMemberInfo;

    public static Trav Create(object o)
    {
        var trav = new Trav();
        if (o is Il2CppObjectBase il2CppObjectBase)
        {
            trav._il2CPPClass = Il2CppType.TypeFromPointer(IL2CPP.il2cpp_object_get_class(il2CppObjectBase.Pointer));
            trav._isIl2CPP = true;
        }
        else
        {
            trav._traverse = Traverse.Create(o);
        }

        trav._o = o;

        return trav;
    }

    private Trav()
    {
    }

    private Trav(Traverse traverse)
    {
        this._traverse = traverse;
    }

    public IEnumerable<string> Fields()
    {
        if (_isIl2CPP)
        {
            foreach (var info in _il2CPPClass.GetFields((BindingFlags)(-1)).ToArray())
            {
                yield return info.Name;
            }

            yield break;
        }

        foreach (var field in _traverse.Fields())
        {
            yield return field;
        }
    }

    public Trav Field(string fld)
    {
        if (_isIl2CPP)
        {
            var trav = Create(_o);

            var fieldInfo = _il2CPPClass.GetField(fld, (BindingFlags)(-1));
            trav._il2CPPMemberInfo = fieldInfo;

            return trav;
        }

        return new Trav(_traverse.Field(fld));
    }

    public bool IsSubclassOf(Type type)
    {
        if (_isIl2CPP)
        {
            if (_il2CPPMemberInfo == null)
            {
                return _il2CPPClass.IsSubclassOf(Il2CppType.From(type));
            }

            if (_il2CPPMemberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.FieldType.IsSubclassOf(Il2CppType.From(type));
            }

            if (_il2CPPMemberInfo is MethodInfo methodInfo)
            {
                return methodInfo.ReturnType.IsSubclassOf(Il2CppType.From(type));
            }

            if (_il2CPPMemberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.PropertyType.IsSubclassOf(Il2CppType.From(type));
            }
        }

        return _traverse.GetValueType().IsSubclassOf(type);
    }

    public T GetValue<T>()
    {
        if (_isIl2CPP)
        {
            if (typeof(T) == typeof(IList))
            {
                var value = GetValue<Il2CppSystem.Collections.IList>();
                return (T)(object)new List2List(value);
            }

            if (_il2CPPMemberInfo == null)
            {
                return _o.Cast<T>();
            }


            if (_il2CPPMemberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.GetValue((Object)_o).Cast<T>();
            }

            if (_il2CPPMemberInfo is MethodInfo methodInfo)
            {
                return methodInfo.Invoke((Object)_o, new Il2CppReferenceArray<Object>(0)).Cast<T>();
            }

            if (_il2CPPMemberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetValue((Object)_o, new Il2CppReferenceArray<Object>(0)).Cast<T>();
            }
        }

        return _traverse.GetValue<T>();
    }

    public interface IIl2CppBridge
    {
        public Object CppObject { get; }
    }

    public void SetValue(object value)
    {
        if (_isIl2CPP)
        {
            if (value is IIl2CppBridge bridge) value = bridge.CppObject;
            if (_il2CPPMemberInfo == null) return;

            if (_il2CPPMemberInfo is FieldInfo fieldInfo)
            {
                fieldInfo.SetValue((Object)_o, (Object)value);
                return;
            }

            if (_il2CPPMemberInfo is MethodInfo methodInfo) return;

            if (_il2CPPMemberInfo is PropertyInfo propertyInfo)
            {
                propertyInfo.GetSetMethod(true)
                    .Invoke((Object)_o, new Il2CppReferenceArray<Object>([value.Cast<Object>()]));
                return;
            }
        }

        _traverse.SetValue(value);
    }

    public object GetValue()
    {
        if (_isIl2CPP)
        {
            if (_il2CPPMemberInfo == null)
            {
                return _o;
            }


            if (_il2CPPMemberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.GetValue((Object)_o);
            }

            if (_il2CPPMemberInfo is MethodInfo methodInfo)
            {
                return methodInfo.Invoke((Object)_o, new Il2CppReferenceArray<Object>(0));
            }

            if (_il2CPPMemberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetValue((Object)_o, new Il2CppReferenceArray<Object>(0));
            }
        }

        return _traverse.GetValue();
    }

    public Trav Method(string method)
    {
        if (_isIl2CPP)
        {
            var trav = Create(_o);
            var methodInfo = _il2CPPClass.GetMethod(method, (BindingFlags)(-1));
            trav._il2CPPMemberInfo = methodInfo;
            return trav;
        }

        return new Trav(_traverse.Method(method));
    }

    public bool FieldExists()
    {
        if (_isIl2CPP)
        {
            return _il2CPPMemberInfo is FieldInfo;
        }

        return _traverse.FieldExists();
    }
}