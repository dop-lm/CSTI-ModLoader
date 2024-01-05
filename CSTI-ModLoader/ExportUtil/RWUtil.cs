using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ModLoader.ExportUtil;

public static class RWUtil
{
    public static void Write(this BinaryWriter writer, List<string> data)
    {
        writer.Write(data.Count);
        foreach (var str in data)
        {
            writer.Write(str);
        }
    }

    public static List<string> ReadListStr(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var list = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(reader.ReadString());
        }

        return list;
    }

    public static void Write(this BinaryWriter writer, Vector2[] data)
    {
        writer.Write(data.Length);
        foreach (var v2 in data)
        {
            writer.Write(v2.x);
            writer.Write(v2.y);
        }
    }

    public static Vector2[] ReadVector2s(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var vector2s = new List<Vector2>(count);
        for (var i = 0; i < count; i++)
        {
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            vector2s.Add(new Vector2(x, y));
        }

        return vector2s.ToArray();
    }

    public static void Write(this BinaryWriter writer, Vector3[] data)
    {
        writer.Write(data.Length);
        foreach (var v3 in data)
        {
            writer.Write(v3.x);
            writer.Write(v3.y);
            writer.Write(v3.z);
        }
    }

    public static Vector3[] ReadVector3s(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var vector3s = new List<Vector3>(count);
        for (var i = 0; i < count; i++)
        {
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            vector3s.Add(new Vector3(x, y, z));
        }

        return vector3s.ToArray();
    }

    public static void Write(this BinaryWriter writer, List<Vector2[]> data)
    {
        writer.Write(data.Count);
        foreach (var v2s in data)
        {
            writer.Write(v2s);
        }
    }

    public static List<Vector2[]> ReadLiVector2s(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var liVector2s = new List<Vector2[]>(count);
        for (var i = 0; i < count; i++)
        {
            liVector2s.Add(reader.ReadVector2s());
        }

        return liVector2s;
    }

    public static void Write(this BinaryWriter writer, List<Vector3[]> data)
    {
        writer.Write(data.Count);
        foreach (var v3s in data)
        {
            writer.Write(v3s);
        }
    }

    public static List<Vector3[]> ReadLiVector3s(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var liVector3s = new List<Vector3[]>(count);
        for (var i = 0; i < count; i++)
        {
            liVector3s.Add(reader.ReadVector3s());
        }

        return liVector3s;
    }

    public static void Write(this BinaryWriter writer, List<int> data)
    {
        writer.Write(data.Count);
        foreach (var i in data)
        {
            writer.Write(i);
        }
    }

    public static List<int> ReadLiInt(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var liInt = new List<int>(count);
        for (var i = 0; i < count; i++)
        {
            liInt.Add(reader.ReadInt32());
        }

        return liInt;
    }

    public static void Write(this BinaryWriter writer, List<List<Vector2[]>> data)
    {
        writer.Write(data.Count);
        foreach (var v2s in data)
        {
            writer.Write(v2s);
        }
    }

    public static List<List<Vector2[]>> ReadLiLiVector2s(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var liLiVector2s = new List<List<Vector2[]>>(count);
        for (var i = 0; i < count; i++)
        {
            liLiVector2s.Add(reader.ReadLiVector2s());
        }

        return liLiVector2s;
    }

    public static void Write(this BinaryWriter writer, ushort[] data)
    {
        writer.Write(data.Length);
        foreach (var us in data)
        {
            writer.Write(us);
        }
    }

    public static ushort[] ReadUShorts(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var list = new List<ushort>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(reader.ReadUInt16());
        }

        return list.ToArray();
    }

    public static void Write(this BinaryWriter writer, List<ushort[]> data)
    {
        writer.Write(data.Count);
        foreach (var us in data)
        {
            writer.Write(us);
        }
    }

    public static List<ushort[]> ReadLiUShorts(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var us = new List<ushort[]>(count);
        for (var i = 0; i < count; i++)
        {
            us.Add(reader.ReadUShorts());
        }

        return us;
    }

    public static void Write(this BinaryWriter writer, Rect[] data)
    {
        writer.Write(data.Length);
        foreach (var rect in data)
        {
            writer.Write(rect.width);
            writer.Write(rect.height);
            writer.Write(rect.x);
            writer.Write(rect.y);
        }
    }

    public static Rect[] ReadRects(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var list = new List<Rect>(count);
        for (var i = 0; i < count; i++)
        {
            var width = reader.ReadSingle();
            var height = reader.ReadSingle();
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            list.Add(new Rect(x, y, width, height));
        }

        return list.ToArray();
    }
}