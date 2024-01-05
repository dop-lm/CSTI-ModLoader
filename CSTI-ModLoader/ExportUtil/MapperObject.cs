using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LitJson;

namespace ModLoader.ExportUtil;

public abstract class MapperItem(StringMapper mapper) : KVProvider
{
    public abstract void WriteSelf(BinaryWriter writer);
    public abstract void ReadSelf(BinaryReader reader);


    public void Write(BinaryWriter writer)
    {
        switch (this)
        {
            case MapperObject:
                writer.Write((byte) 1);
                WriteSelf(writer);
                break;
            case MapperList:
                writer.Write((byte) 2);
                WriteSelf(writer);
                break;
            case ObjBool:
                writer.Write((byte) 3);
                WriteSelf(writer);
                break;
            case ObjDouble:
                writer.Write((byte) 4);
                WriteSelf(writer);
                break;
            case ObjInt:
                writer.Write((byte) 5);
                WriteSelf(writer);
                break;
            case ObjLong:
                writer.Write((byte) 6);
                WriteSelf(writer);
                break;
            case ObjString:
                writer.Write((byte) 7);
                WriteSelf(writer);
                break;
        }
    }

    public static MapperItem? Read(BinaryReader reader, StringMapper mapper)
    {
        var bType = reader.ReadByte();
        switch (bType)
        {
            case 1:
                var mapperObject = new MapperObject(mapper);
                mapperObject.ReadSelf(reader);
                return mapperObject;
            case 2:
                var mapperList = new MapperList(mapper);
                mapperList.ReadSelf(reader);
                return mapperList;
            case 3:
                var objBool = new ObjBool(mapper);
                objBool.ReadSelf(reader);
                return objBool;
            case 4:
                var objDouble = new ObjDouble(mapper);
                objDouble.ReadSelf(reader);
                return objDouble;
            case 5:
                var objInt = new ObjInt(mapper);
                objInt.ReadSelf(reader);
                return objInt;
            case 6:
                var objLong = new ObjLong(mapper);
                objLong.ReadSelf(reader);
                return objLong;
            case 7:
                var objString = new ObjString(mapper);
                objString.ReadSelf(reader);
                return objString;
        }

        return null;
    }

    public readonly StringMapper Mapper = mapper;

    public static MapperItem CreateByJson(StringMapper mapper, JsonData data)
    {
        if (data.IsObject)
        {
            var mapperObject = new MapperObject(mapper);
            mapperObject.Init(data);
            return mapperObject;
        }

        if (data.IsArray)
        {
            var mapperObject = new MapperList(mapper);
            mapperObject.Init(data);
            return mapperObject;
        }

        if (data.IsLong)
        {
            var mapperObject = new ObjLong(mapper);
            mapperObject.Init(data);
            return mapperObject;
        }

        if (data.IsInt)
        {
            var mapperObject = new ObjInt(mapper);
            mapperObject.Init(data);
            return mapperObject;
        }

        if (data.IsBoolean)
        {
            var mapperObject = new ObjBool(mapper);
            mapperObject.Init(data);
            return mapperObject;
        }

        if (data.IsString)
        {
            var mapperObject = new ObjString(mapper);
            mapperObject.Init(data);
            return mapperObject;
        }

        if (data.IsDouble)
        {
            var mapperObject = new ObjDouble(mapper);
            mapperObject.Init(data);
            return mapperObject;
        }

        return null!;
    }

    public void Init(JsonData data)
    {
        Mapper.CollectJson(data);
        InitSelf(data);
    }

    protected abstract void InitSelf(JsonData data);

    public override bool IsObject => this is MapperObject;
    public override bool IsArray => this is MapperList;
    public override bool IsString => this is ObjString;
    public override bool IsInt => this is ObjInt or ObjLong;
    public override bool IsBoolean => this is ObjBool;

    public override bool ContainsKey(string key)
    {
        if (!IsObject) return false;
        var index = Mapper.GetIndex(key);
        return ((MapperObject) this).Items.ContainsKey(index);
    }

    public override IEnumerable<string> Keys
    {
        get { return !IsObject ? [] : ((MapperObject) this).Items.Keys.Select(i => Mapper.GetKey(i)); }
    }

    public override int Count
    {
        get
        {
            if (IsObject)
            {
                return ((MapperObject) this).Items.Count;
            }

            return IsArray ? ((MapperList) this).Items.Count : 0;
        }
    }

    public override int Int
    {
        get
        {
            switch (this)
            {
                case ObjInt i:
                    return i.Val;
                case ObjLong l:
                    return (int) l.Val;
            }

            return 0;
        }
    }

    public override bool Bool => IsBoolean && ((ObjBool) this).Val;
    public override string String => IsString ? Mapper.GetKey(((ObjString) this).Val) : "";

    public override KVProvider this[string key]
    {
        get
        {
            if (!IsObject) return Null;
            var index = Mapper.GetIndex(key);
            if (((MapperObject) this).Items.TryGetValue(index, out var item))
            {
                return item;
            }

            return Null;
        }
    }

    public override KVProvider this[int index]
    {
        get
        {
            if (this is not MapperList list) return Null;
            if (index >= 0 && index < list.Items.Count)
            {
                return list[index];
            }

            return Null;
        }
    }

    public abstract void ToJson(StringBuilder stringBuilder);

    public override string ToJson()
    {
        var stringBuilder = new StringBuilder();
        ToJson(stringBuilder);
        return stringBuilder.ToString();
    }

    public override string ToString()
    {
        return IsString ? String : ToJson();
    }
}

public class ObjInt(StringMapper mapper) : MapperItem(mapper)
{
    public int Val;

    public override void WriteSelf(BinaryWriter writer)
    {
        writer.Write(Val);
    }

    public override void ReadSelf(BinaryReader reader)
    {
        Val = reader.ReadInt32();
    }

    protected override void InitSelf(JsonData data)
    {
        if (data.IsInt || data.IsLong)
        {
            Val = (int) data;
        }
    }

    public override void ToJson(StringBuilder stringBuilder)
    {
        stringBuilder.Append(Val);
    }
}

public class ObjLong(StringMapper mapper) : MapperItem(mapper)
{
    public long Val;

    public override void WriteSelf(BinaryWriter writer)
    {
        writer.Write(Val);
    }

    public override void ReadSelf(BinaryReader reader)
    {
        Val = reader.ReadInt64();
    }

    protected override void InitSelf(JsonData data)
    {
        if (data.IsInt || data.IsLong)
        {
            Val = (long) data;
        }
    }

    public override void ToJson(StringBuilder stringBuilder)
    {
        stringBuilder.Append(Val);
    }
}

public class ObjBool(StringMapper mapper) : MapperItem(mapper)
{
    public bool Val;

    public override void WriteSelf(BinaryWriter writer)
    {
        writer.Write(Val);
    }

    public override void ReadSelf(BinaryReader reader)
    {
        Val = reader.ReadBoolean();
    }

    protected override void InitSelf(JsonData data)
    {
        if (data.IsBoolean)
        {
            Val = (bool) data;
        }
    }

    public override void ToJson(StringBuilder stringBuilder)
    {
        stringBuilder.Append(Val ? "true" : "false");
    }
}

public class ObjString(StringMapper mapper) : MapperItem(mapper)
{
    public int Val = -1;

    public override void WriteSelf(BinaryWriter writer)
    {
        writer.Write(Val);
    }

    public override void ReadSelf(BinaryReader reader)
    {
        Val = reader.ReadInt32();
    }

    protected override void InitSelf(JsonData data)
    {
        if (data.IsString)
        {
            Val = Mapper.GetIndex((string) data);
        }
    }

    public override void ToJson(StringBuilder stringBuilder)
    {
        stringBuilder.Append('\"');
        if (Mapper.GetKey(Val) is { } s)
        {
            stringBuilder.Append(s.Replace("\n", "\\n"));
        }
        else
        {
            stringBuilder.Append("");
        }

        stringBuilder.Append('\"');
    }
}

public class ObjDouble(StringMapper mapper) : MapperItem(mapper)
{
    public double Val;

    public override void WriteSelf(BinaryWriter writer)
    {
        writer.Write(Val);
    }

    public override void ReadSelf(BinaryReader reader)
    {
        Val = reader.ReadDouble();
    }

    protected override void InitSelf(JsonData data)
    {
        if (data.IsDouble)
        {
            Val = (double) data;
        }
    }

    public override void ToJson(StringBuilder stringBuilder)
    {
        stringBuilder.Append(Val);
    }
}

public class MapperObject(StringMapper mapper) : MapperItem(mapper)
{
    public readonly Dictionary<int, MapperItem> Items = [];

    public override IEnumerable<string> Keys
    {
        get
        {
            foreach (var key in Items.Keys)
            {
                yield return Mapper.GetKey(key);
            }
        }
    }

    public override KVProvider this[string key]
    {
        get
        {
            if (Items.TryGetValue(Mapper.GetIndex(key), out var item))
                return item;

            return Null;
        }
    }

    public override void WriteSelf(BinaryWriter writer)
    {
        writer.Write(Items.Count);
        foreach (var (i, val) in Items)
        {
            writer.Write(i);
            val.Write(writer);
        }
    }

    public override void ReadSelf(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var iKey = reader.ReadInt32();
            var mapperItem = Read(reader, Mapper);
            Items[iKey] = mapperItem;
        }
    }

    protected override void InitSelf(JsonData data)
    {
        if (!data.IsObject) return;
        foreach (var key in data.Keys)
        {
            Items[Mapper.GetIndex(key)] = CreateByJson(Mapper, data[key]);
        }
    }

    public override void ToJson(StringBuilder stringBuilder)
    {
        stringBuilder.Append('{');
        var list = Items.Keys.ToList();
        for (var index = 0; index < list.Count; index++)
        {
            var i = list[index];
            var val = Items[i];
            stringBuilder.Append('\"');
            stringBuilder.Append(Mapper.GetKey(i)!.Replace("\n", "\\n"));
            stringBuilder.Append("\":");
            val.ToJson(stringBuilder);
            if (index + 1 < list.Count)
            {
                stringBuilder.Append(',');
            }
        }

        stringBuilder.Append('}');
    }
}

public class MapperList(StringMapper mapper) : MapperItem(mapper)
{
    public readonly List<MapperItem> Items = [];

    public override KVProvider this[int index]
    {
        get
        {
            if (index >= 0 && index < Items.Count)
            {
                return Items[index];
            }

            return Null;
        }
    }

    public override void WriteSelf(BinaryWriter writer)
    {
        writer.Write(Items.Count);
        foreach (var item in Items)
        {
            item.Write(writer);
        }
    }

    public override void ReadSelf(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var mapperItem = Read(reader, Mapper);
            Items.Add(mapperItem);
        }
    }

    protected override void InitSelf(JsonData data)
    {
        if (!data.IsArray) return;
        for (var i = 0; i < data.Count; i++)
        {
            Items.Add(CreateByJson(Mapper, data[i]));
        }
    }

    public override void ToJson(StringBuilder stringBuilder)
    {
        stringBuilder.Append('[');
        for (var index = 0; index < Items.Count; index++)
        {
            var val = Items[index];
            val.ToJson(stringBuilder);
            if (index + 1 < Items.Count)
            {
                stringBuilder.Append(',');
            }
        }

        stringBuilder.Append(']');
    }
}