using System.Collections.Generic;
using LitJson;

namespace ModLoader.ExportUtil;

public class NullKV : KVProvider
{
    public override bool IsObject { get; }
    public override bool IsArray { get; }
    public override bool IsString { get; }
    public override bool IsInt { get; }
    public override bool IsBoolean { get; }

    public override string ToJson()
    {
        throw new System.NotImplementedException();
    }

    public override bool ContainsKey(string key)
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerable<string> Keys { get; }
    public override int Count { get; }

    public override KVProvider this[string key] => throw new System.NotImplementedException();

    public override KVProvider this[int index] => throw new System.NotImplementedException();

    public override int Int { get; }
    public override bool Bool { get; }
    public override string String { get; }
}

public abstract class KVProvider
{
    public static readonly NullKV Null = new();
    public abstract bool IsObject { get; }
    public abstract bool IsArray { get; }
    public abstract bool IsString { get; }
    public abstract bool IsInt { get; }
    public abstract bool IsBoolean { get; }
    public abstract string ToJson();
    public abstract bool ContainsKey(string key);
    public abstract IEnumerable<string> Keys { get; }
    public abstract int Count { get; }
    public abstract KVProvider this[string key] { get; }
    public abstract KVProvider this[int index] { get; }

    public abstract int Int { get; }

    public static explicit operator int(KVProvider provider)
    {
        return provider.IsInt ? provider.Int : 0;
    }

    public abstract bool Bool { get; }
    public abstract string String { get; }

    public static explicit operator bool(KVProvider provider)
    {
        return provider is {IsBoolean: true, Bool: true};
    }

    public static explicit operator string(KVProvider provider)
    {
        return provider.IsString ? provider.String : "";
    }
}

public class JsonKVProvider : KVProvider
{
    public readonly JsonData Data;

    public JsonKVProvider(JsonData jsonData)
    {
        Data = jsonData;
    }

    public override bool IsObject => Data.IsObject;
    public override bool IsArray => Data.IsArray;

    public override bool IsString => Data.IsString;

    public override bool IsInt => Data.IsInt;

    public override bool IsBoolean => Data.IsBoolean;

    public override string ToJson()
    {
        return Data.ToJson();
    }

    public override bool ContainsKey(string key)
    {
        return Data.ContainsKey(key);
    }

    public override IEnumerable<string> Keys => Data.Keys;

    public override int Count => Data.Count;

    public override KVProvider this[string key] => new JsonKVProvider(Data[key]);

    public override KVProvider this[int index] => new JsonKVProvider(Data[index]);

    public override int Int => Data.IsInt ? (int) Data : 0;
    public override bool Bool => Data.IsBoolean && (bool) Data;
    public override string String => Data.IsString ? (string) Data : Data.ToString();

    public override string ToString()
    {
        return String;
    }
}