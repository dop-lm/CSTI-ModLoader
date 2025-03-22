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

    public Trav Field(string fld, bool withStatic = true)
    {
        if (_isIl2CPP)
        {
            var trav = Create(_o);

            var fieldInfo = _il2CPPClass.GetField(fld, (BindingFlags)(-1));
            trav._il2CPPMemberInfo = fieldInfo;
            if (!withStatic && fieldInfo.IsStatic) trav._il2CPPMemberInfo = null;

            return trav;
        }

        return new Trav(_traverse.Field(fld));
    }

    public bool IsSubclassOf(Type type)
    {
        if (_isIl2CPP)
        {
            var cppType = Il2CppType.From(type, false);
            if (cppType == null) return false;
            if (_il2CPPMemberInfo == null)
            {
                return _il2CPPClass.IsSubclassOf(cppType);
            }

            if (_il2CPPMemberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.FieldType.IsSubclassOf(cppType);
            }

            if (_il2CPPMemberInfo is MethodInfo methodInfo)
            {
                return methodInfo.ReturnType.IsSubclassOf(cppType);
            }

            if (_il2CPPMemberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.PropertyType.IsSubclassOf(cppType);
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
                return _o.SafeCast<T>();
            }


            if (_il2CPPMemberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.GetValue(_o.SafeCast<Object>()).SafeCast<T>();
            }

            if (_il2CPPMemberInfo is MethodInfo methodInfo)
            {
                return methodInfo.Invoke(_o.SafeCast<Object>(), new Il2CppReferenceArray<Object>(0)).SafeCast<T>();
            }

            if (_il2CPPMemberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetValue(_o.SafeCast<Object>(), new Il2CppReferenceArray<Object>(0)).SafeCast<T>();
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
                fieldInfo.SetValue(_o.SafeCast<Object>(), value.SafeCast<Object>());
                return;
            }

            if (_il2CPPMemberInfo is MethodInfo methodInfo) return;

            if (_il2CPPMemberInfo is PropertyInfo propertyInfo)
            {
                propertyInfo.GetSetMethod(true)
                    .Invoke(_o.SafeCast<Object>(), new Il2CppReferenceArray<Object>([value.SafeCast<Object>()]));
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
                return fieldInfo.GetValue(_o.SafeCast<Object>());
            }

            if (_il2CPPMemberInfo is MethodInfo methodInfo)
            {
                return methodInfo.Invoke(_o.SafeCast<Object>(), new Il2CppReferenceArray<Object>(0));
            }

            if (_il2CPPMemberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetValue(_o.SafeCast<Object>(), new Il2CppReferenceArray<Object>(0));
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

    public override bool Equals(object? obj)
    {
        if (obj is not Trav trav) return false;
        if (_isIl2CPP)
        {
            return trav._isIl2CPP && trav._il2CPPClass == _il2CPPClass && trav._il2CPPMemberInfo == _il2CPPMemberInfo;
        }

        return _traverse.ToString() == trav._traverse.ToString();
    }

    public override int GetHashCode()
    {
        if (_isIl2CPP)
        {
            return (int)(10101 + (nint)_il2CPPClass.Pointer);
        }

        return _traverse.ToString().GetHashCode();
    }
}