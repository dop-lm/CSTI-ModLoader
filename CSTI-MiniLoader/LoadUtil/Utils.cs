using System;
using System.Collections;
using System.Collections.Generic;
using Cpp2IL.Core;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using IList = Il2CppSystem.Collections.IList;
using Object = Il2CppSystem.Object;

namespace CSTI_MiniLoader.LoadUtil;

public static class Utils
{
    public static Transform Find(this Transform parent, string name)
    {
        foreach (var t in parent)
        {
            if (t is Transform tr && tr.name == name)
            {
                return tr;
            }
        }

        return parent;
    }

    public static List<T> ToList<T>(this IEnumerable<T> array)
    {
        return [..array];
    }

    public static List<object> ToList(this IEnumerable array)
    {
        return [..array];
    }

    public static T First<T>(this IEnumerable<T> array)
    {
        foreach (var t in array)
        {
            return t;
        }

        return default;
    }

    public static IEnumerable<T> Intersect<T>(this List<T> ea, IEnumerable<T> eb)
    {
        foreach (var t in eb)
        {
            if (ea.Contains(t))
            {
                yield return t;
            }
        }
    }

    public static string Uid(this UniqueIDScriptable o)
    {
        return o.UniqueID;
    }

    public static T Cast<T>(this object o)
    {
        if (Il2CppType.Of<T>() == null)
        {
            return (T)o;
        }

        if (o is Il2CppObjectBase il2CppObjectBase)
        {
            return (T)Activator.CreateInstance(typeof(T), IL2CPP.Il2CppObjectBaseToPtr(il2CppObjectBase));
        }

        return default;
    }
}

public class List2List : System.Collections.IList,Trav.IIl2CppBridge
{
    private readonly IList _list;

    public List2List(IList list)
    {
        _list = list;
    }

    public IEnumerator GetEnumerator()
    {
        yield break;
    }

    public void CopyTo(Array array, int index)
    {
    }

    public int Count { get; }
    public object SyncRoot { get; }
    public bool IsSynchronized { get; }

    public int Add(object? value)
    {
        if (value is Il2CppSystem.Object il2CppObject)
        {
            _list.Add(il2CppObject);
        }

        return Count;
    }

    public bool Contains(object? value)
    {
        if (value is Il2CppSystem.Object il2CppObject)
        {
            return _list.Contains(il2CppObject);
        }

        return false;
    }

    public void Clear()
    {
        _list.Clear();
    }

    public int IndexOf(object value)
    {
        if (value is Il2CppSystem.Object il2CppObject)
        {
            return _list.IndexOf(il2CppObject);
        }

        return -1;
    }

    public void Insert(int index, object value)
    {
        if (value is Il2CppSystem.Object il2CppObject)
        {
            _list.Insert(index, il2CppObject);
        }
    }

    public void Remove(object value)
    {
        if (value is Il2CppSystem.Object il2CppObject)
        {
            _list.Remove(il2CppObject);
        }
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    public object this[int index]
    {
        get => _list.get_Item(index);
        set
        {
            if (value is Il2CppSystem.Object il2CppObject)
            {
                _list.set_Item(index, il2CppObject);
            }
        }
    }

    public bool IsReadOnly => _list.IsReadOnly;
    public bool IsFixedSize => false;
    public Object CppObject => _list.TryCast<Object>();
}