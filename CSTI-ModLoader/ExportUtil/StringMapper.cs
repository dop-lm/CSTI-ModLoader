using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using LitJson;

namespace ModLoader.ExportUtil;

public class StringMapper
{
    public static StringMapper Empty => new();

    public readonly List<string> ListStr = new();
    public readonly Dictionary<string, int> StrList = new();

    public void Write(BinaryWriter writer)
    {
        writer.Write(ListStr.Count);
        foreach (var s in ListStr)
        {
            writer.Write(s);
        }
    }

    public void Read(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var s = reader.ReadString();
            StrList[s] = ListStr.Count;
            ListStr.Add(s);
        }
    }

    public void CollectJson(JsonData data)
    {
        if (data.IsObject)
        {
            foreach (var key in data.Keys)
            {
                Add(key);
                var val = data[key];
                if (val.IsString) Add((string) val);
                else CollectJson(val);
            }
        }
        else if (data.IsArray)
        {
            for (var i = 0; i < data.Count; i++)
            {
                var val = data[i];
                if (val.IsString) Add((string) val);
                else CollectJson(val);
            }
        }
    }

    public void Add(string s)
    {
        if (StrList.ContainsKey(s)) return;
        StrList[s] = ListStr.Count;
        ListStr.Add(s);
    }

    public int GetIndex(string key)
    {
        if (StrList.TryGetValue(key, out var index))
        {
            return index;
        }

        return -1;
    }

    public string GetKey(int index)
    {
        if (ListStr.Count > index && index >= 0)
        {
            return ListStr[index];
        }

        return "";
    }
}