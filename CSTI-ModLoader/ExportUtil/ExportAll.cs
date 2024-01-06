using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using LitJson;
using LZ4;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ModLoader.ExportUtil;

public static class ExportAll
{
    public static bool CheckGenerateAtlas(IEnumerable<Texture2D> texture2Ds, int size)
    {
        var rects = new List<Rect>();
        var vector2s = texture2Ds.Select(texture2D => new Vector2(texture2D.width, texture2D.height)).ToArray();
        var generateAtlas = Texture2D.GenerateAtlas(
            vector2s, 1, size,
            rects);
        if (!generateAtlas) return false;
        for (var i = 0; i < rects.Count; i++)
        {
            if (Math.Abs(rects[i].width - vector2s[i].x) > 0.01) return false;
            if (Math.Abs(rects[i].height - vector2s[i].y) > 0.01) return false;
        }

        return true;
    }

    public static (string, T) RandPop<T>(this Dictionary<string, T> dict, Func<T, float> wFunc)
    {
        if (dict.Count == 0) return default;
        var valueTuples = dict.Select(pair => (pair.Key, pair.Value, weight: wFunc(pair.Value))).ToList();
        var sumW = valueTuples.Sum(tuple => tuple.weight);
        var f = Random.Range(0, sumW);
        var curF = 0f;
        foreach (var (key, value, weight) in valueTuples)
        {
            if (f <= curF + weight)
            {
                dict.Remove(key);
                return (key, value);
            }

            curF += weight;
        }

        var (k, v, _) = valueTuples.Last();
        dict.Remove(k);
        return (k, v);
    }

    internal static List<string> SplitPath(string path, string stop)
    {
        var list = new List<string>();
        while (!path.IsNullOrWhiteSpace())
        {
            var fileName = Path.GetFileName(path);
            if (fileName == stop)
            {
                break;
            }

            list.Insert(0, fileName);
            path = Path.GetDirectoryName(path);
        }

        return list;
    }

    public class ExportArch
    {
        public int ModArchVersion = 3;

        public readonly string ModPath;

        public readonly string ModName;

        public const long MaxBlockSizeForRes = 64 * 1024 * 1024;

        public void CollectObj()
        {
            foreach (var file in Directory.EnumerateFiles(ModPath, "*", SearchOption.AllDirectories))
            {
                var fileExtension = Path.GetExtension(file);
                if (!fileExtension.EndsWith("json", StringComparison.OrdinalIgnoreCase) &&
                    !fileExtension.EndsWith("csv", StringComparison.OrdinalIgnoreCase) &&
                    !fileExtension.EndsWith("lua", StringComparison.OrdinalIgnoreCase) &&
                    !fileExtension.EndsWith("jsonnet", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (fileExtension.EndsWith("csv", StringComparison.OrdinalIgnoreCase))
                {
                    var localBuffer = AllData[ExportArchDataType.Local];
                    var localWriter = new BinaryWriter(localBuffer, Encoding.UTF8, true);
                    localWriter.Write(Path.GetFileName(file));
                    var localFile = File.ReadAllText(file);
                    localWriter.Write(localFile);
                }
                else if (fileExtension.EndsWith("lua", StringComparison.OrdinalIgnoreCase))
                {
                    var luaBuffer = AllData[ExportArchDataType.Lua];
                    var luaWriter = new BinaryWriter(luaBuffer, Encoding.UTF8, true);
                    var splitPath = SplitPath(file, ModName);
                    luaWriter.Write(1);
                    luaWriter.Write(splitPath);
                    var luaFile = File.ReadAllText(file);
                    luaWriter.Write(luaFile);
                }
                else
                {
                    var jsonBuffer = AllData[ExportArchDataType.Jsons];
                    var jsonWriter = new BinaryWriter(jsonBuffer, Encoding.UTF8, true);
                    var splitPath = SplitPath(file, ModName);
                    jsonWriter.Write(1);
                    jsonWriter.Write(splitPath);
                    var jsonFile = File.ReadAllText(file);
                    jsonWriter.Write(jsonFile);
                }
            }

            new BinaryWriter(AllData[ExportArchDataType.Local], Encoding.UTF8, true).Write(LoadArchMod.EndFlg);
            var bytes = AllData[ExportArchDataType.Local].ToArray();
            AllData[ExportArchDataType.Local].Close();
            AllData[ExportArchDataType.Local] = new MemoryStream();
            var maximumOutputSize = LZ4Codec.MaximumOutputLength(bytes.Length);
            var buffer = new byte[maximumOutputSize];
            var encodeLen = LZ4Codec.Encode(bytes, 0, bytes.Length, buffer, 0, buffer.Length);
            AllLz4Data[ExportArchDataType.Local].Add((buffer, encodeLen, bytes.Length));

            new BinaryWriter(AllData[ExportArchDataType.Jsons], Encoding.UTF8, true).Write(0);
            bytes = AllData[ExportArchDataType.Jsons].ToArray();
            AllData[ExportArchDataType.Jsons].Close();
            AllData[ExportArchDataType.Jsons] = new MemoryStream();
            maximumOutputSize = LZ4Codec.MaximumOutputLength(bytes.Length);
            buffer = new byte[maximumOutputSize];
            encodeLen = LZ4Codec.Encode(bytes, 0, bytes.Length, buffer, 0, buffer.Length);
            AllLz4Data[ExportArchDataType.Jsons].Add((buffer, encodeLen, bytes.Length));

            new BinaryWriter(AllData[ExportArchDataType.Lua], Encoding.UTF8, true).Write(0);
            bytes = AllData[ExportArchDataType.Lua].ToArray();
            AllData[ExportArchDataType.Lua].Close();
            AllData[ExportArchDataType.Lua] = new MemoryStream();
            maximumOutputSize = LZ4Codec.MaximumOutputLength(bytes.Length);
            buffer = new byte[maximumOutputSize];
            encodeLen = LZ4Codec.Encode(bytes, 0, bytes.Length, buffer, 0, buffer.Length);
            AllLz4Data[ExportArchDataType.Lua].Add((buffer, encodeLen, bytes.Length));
        }

        public void CollectObjV3()
        {
            var mapper = StringMapper.Empty;
            var cacheJson = new Queue<(List<string> splitPath, MapperObject o)>();
            foreach (var file in Directory.EnumerateFiles(ModPath, "*", SearchOption.AllDirectories))
            {
                var fileExtension = Path.GetExtension(file);
                if (!fileExtension.EndsWith("json", StringComparison.OrdinalIgnoreCase) &&
                    !fileExtension.EndsWith("csv", StringComparison.OrdinalIgnoreCase) &&
                    !fileExtension.EndsWith("lua", StringComparison.OrdinalIgnoreCase) &&
                    !fileExtension.EndsWith("jsonnet", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (fileExtension.EndsWith("csv", StringComparison.OrdinalIgnoreCase))
                {
                    var localBuffer = AllData[ExportArchDataType.Local];
                    var localWriter = new BinaryWriter(localBuffer, Encoding.UTF8, true);
                    localWriter.Write(Path.GetFileName(file));
                    var localFile = File.ReadAllText(file);
                    localWriter.Write(localFile);
                }
                else if (fileExtension.EndsWith("lua", StringComparison.OrdinalIgnoreCase))
                {
                    var luaBuffer = AllData[ExportArchDataType.Lua];
                    var luaWriter = new BinaryWriter(luaBuffer, Encoding.UTF8, true);
                    var splitPath = SplitPath(file, ModName);
                    luaWriter.Write(1);
                    luaWriter.Write(splitPath);
                    var luaFile = File.ReadAllText(file);
                    luaWriter.Write(luaFile);
                }
                else
                {
                    var splitPath = SplitPath(file, ModName);
                    var jsonFile = File.ReadAllText(file);
                    var jsonData = JsonMapper.ToObject(jsonFile);
                    var mapperObject = new MapperObject(mapper);
                    mapperObject.Init(jsonData);
                    cacheJson.Enqueue((splitPath, mapperObject));
                }
            }

            new BinaryWriter(AllData[ExportArchDataType.Local], Encoding.UTF8, true).Write(LoadArchMod.EndFlg);
            var bytes = AllData[ExportArchDataType.Local].ToArray();
            AllData[ExportArchDataType.Local].Close();
            AllData[ExportArchDataType.Local] = new MemoryStream();
            var maximumOutputSize = LZ4Codec.MaximumOutputLength(bytes.Length);
            var buffer = new byte[maximumOutputSize];
            var encodeLen = LZ4Codec.Encode(bytes, 0, bytes.Length, buffer, 0, buffer.Length);
            AllLz4Data[ExportArchDataType.Local].Add((buffer, encodeLen, bytes.Length));

            var binaryWriter = new BinaryWriter(AllData[ExportArchDataType.Jsons], Encoding.UTF8, true);
            binaryWriter.Write(2);
            mapper.Write(binaryWriter);
            while (cacheJson.Count > 0)
            {
                var (splitPath, o) = cacheJson.Dequeue();
                binaryWriter.Write(1);
                binaryWriter.Write(splitPath);
                o.Write(binaryWriter);
            }

            binaryWriter.Write(0);
            bytes = AllData[ExportArchDataType.Jsons].ToArray();
            AllData[ExportArchDataType.Jsons].Close();
            AllData[ExportArchDataType.Jsons] = new MemoryStream();
            maximumOutputSize = LZ4Codec.MaximumOutputLength(bytes.Length);
            buffer = new byte[maximumOutputSize];
            encodeLen = LZ4Codec.Encode(bytes, 0, bytes.Length, buffer, 0, buffer.Length);
            AllLz4Data[ExportArchDataType.Jsons].Add((buffer, encodeLen, bytes.Length));

            new BinaryWriter(AllData[ExportArchDataType.Lua], Encoding.UTF8, true).Write(0);
            bytes = AllData[ExportArchDataType.Lua].ToArray();
            AllData[ExportArchDataType.Lua].Close();
            AllData[ExportArchDataType.Lua] = new MemoryStream();
            maximumOutputSize = LZ4Codec.MaximumOutputLength(bytes.Length);
            buffer = new byte[maximumOutputSize];
            encodeLen = LZ4Codec.Encode(bytes, 0, bytes.Length, buffer, 0, buffer.Length);
            AllLz4Data[ExportArchDataType.Lua].Add((buffer, encodeLen, bytes.Length));
        }

        public void CollectImgV2()
        {
            var ImgPath = Path.Combine(ModPath, "Resource", "Picture");
            if (!Directory.Exists(ImgPath)) return;
            var curTexPackSize = 1024;
            var texture2D = new Texture2D(curTexPackSize, curTexPackSize);
            var allCacheTexture = new Dictionary<string, Texture2D>();
            var cacheTexture = new Dictionary<string, Texture2D>();
            foreach (var file in Directory.GetFiles(ImgPath, "*", SearchOption.AllDirectories))
            {
                var fileExtension = Path.GetExtension(file);
                if (!fileExtension.EndsWith("png", StringComparison.OrdinalIgnoreCase) &&
                    !fileExtension.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) &&
                    !fileExtension.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var tmp = new MemoryStream();
                    using var img = File.OpenRead(file);
                    img.CopyTo(tmp);
                    var bytes = tmp.ToArray();
                    tmp.Close();

                    var tmpTex = new Texture2D(0, 0);
                    tmpTex.LoadImage(bytes);
                    allCacheTexture[file] = tmpTex;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            var writer = new BinaryWriter(AllData[ExportArchDataType.Img], Encoding.UTF8, true);
            while (allCacheTexture.Count > 0)
            {
                var (s, pop) = allCacheTexture.RandPop(t => 8192f / (t.height * t.width));
                var cacheCurTexPackSize = curTexPackSize;
                while (cacheCurTexPackSize < 8192)
                {
                    if (CheckGenerateAtlas(cacheTexture.Values.Append(pop), cacheCurTexPackSize)) break;
                    cacheCurTexPackSize *= 2;
                }


                if (CheckGenerateAtlas(cacheTexture.Values.Append(pop), cacheCurTexPackSize))
                {
                    curTexPackSize = cacheCurTexPackSize;
                    texture2D.Resize(curTexPackSize, curTexPackSize);
                }

                if (!CheckGenerateAtlas(cacheTexture.Values.Append(pop), curTexPackSize))
                {
                    if (cacheTexture.Count == 0)
                    {
                        pop.Compress(true);
                        var rawTextureData = pop.GetRawTextureData();
                        writer.Write(1);
                        writer.Write(Path.GetFileName(s));
                        writer.Write(pop.width);
                        writer.Write(pop.height);
                        writer.Write((int) pop.format);
                        writer.Write(rawTextureData.Length);
                        writer.Write(rawTextureData);
                        Object.DestroyImmediate(pop);
                    }
                    else
                    {
                        var packTextures = texture2D.PackTextures(cacheTexture.Values.ToArray(),
                            1, curTexPackSize, false);
                        texture2D.Compress(true);
                        var rawTextureData = texture2D.GetRawTextureData();
                        writer.Write(2);
                        writer.Write(packTextures);
                        texture2D.name =
                            $"ImgPack_{ModName}_{Random.Range(int.MinValue, int.MaxValue):X}_{Random.Range(int.MinValue, int.MaxValue):X}";
                        writer.Write(texture2D.name);
                        writer.Write(cacheTexture.Keys.ToList());
                        writer.Write((int) texture2D.format);
                        writer.Write(texture2D.width);
                        writer.Write(texture2D.height);
                        writer.Write(rawTextureData.Length);
                        writer.Write(rawTextureData);
                        cacheTexture.Clear();
                        curTexPackSize = 1024;
                        Object.DestroyImmediate(texture2D);
                        texture2D = new Texture2D(curTexPackSize, curTexPackSize);
                        allCacheTexture[s] = pop;
                    }

                    if (AllData[ExportArchDataType.Img].Length > MaxBlockSizeForRes)
                    {
                        writer.Write(0);
                        var maximumOutputSize =
                            LZ4Codec.MaximumOutputLength((int) AllData[ExportArchDataType.Img].Length);
                        var buffer = new byte[maximumOutputSize];
                        var memoryStream = AllData[ExportArchDataType.Img];
                        var bytes = memoryStream.ToArray();
                        AllData[ExportArchDataType.Img].Close();
                        AllData[ExportArchDataType.Img] = new MemoryStream();
                        var encodeLen = LZ4Codec.Encode(bytes, 0, bytes.Length, buffer, 0, buffer.Length);
                        AllLz4Data[ExportArchDataType.Img].Add((buffer, encodeLen, bytes.Length));
                        writer = new BinaryWriter(AllData[ExportArchDataType.Img]);
                    }
                }
                else
                {
                    cacheTexture[s] = pop;
                }

                if (allCacheTexture.Count == 0)
                {
                    if (cacheTexture.Count != 0)
                    {
                        var packTextures = texture2D.PackTextures(cacheTexture.Values.ToArray(),
                            1, curTexPackSize, false);
                        texture2D.Compress(true);
                        var rawTextureData = texture2D.GetRawTextureData();
                        writer.Write(2);
                        writer.Write(packTextures);
                        texture2D.name =
                            $"ImgPack_{ModName}_{Random.Range(int.MinValue, int.MaxValue):X}_{Random.Range(int.MinValue, int.MaxValue):X}";
                        writer.Write(texture2D.name);
                        writer.Write(cacheTexture.Keys.ToList());
                        writer.Write((int) texture2D.format);
                        writer.Write(texture2D.width);
                        writer.Write(texture2D.height);
                        writer.Write(rawTextureData.Length);
                        writer.Write(rawTextureData);
                        cacheTexture.Clear();
                        curTexPackSize = 1024;
                        Object.DestroyImmediate(texture2D);
                        texture2D = new Texture2D(curTexPackSize, curTexPackSize);
                    }

                    writer.Write(0);
                    var maximumOutputSize =
                        LZ4Codec.MaximumOutputLength((int) AllData[ExportArchDataType.Img].Length);
                    var buffer = new byte[maximumOutputSize];
                    var memoryStream = AllData[ExportArchDataType.Img];
                    var bytes = memoryStream.ToArray();
                    AllData[ExportArchDataType.Img].Close();
                    AllData[ExportArchDataType.Img] = new MemoryStream();
                    var encodeLen = LZ4Codec.Encode(bytes, 0, bytes.Length, buffer, 0, buffer.Length);
                    AllLz4Data[ExportArchDataType.Img].Add((buffer, encodeLen, bytes.Length));
                    writer = new BinaryWriter(AllData[ExportArchDataType.Img], Encoding.UTF8, true);
                }
            }
        }

        public void CollectImg()
        {
            var ImgPath = Path.Combine(ModPath, "Resource", "Picture");
            if (!Directory.Exists(ImgPath)) return;
            var texture2D = new Texture2D(0, 0);
            foreach (var file in Directory.GetFiles(ImgPath, "*", SearchOption.AllDirectories))
            {
                var fileExtension = Path.GetExtension(file);
                if (!fileExtension.EndsWith("png", StringComparison.OrdinalIgnoreCase) &&
                    !fileExtension.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) &&
                    !fileExtension.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var tmp = new MemoryStream();
                    using var img = File.OpenRead(file);
                    img.CopyTo(tmp);
                    var bytes = tmp.ToArray();
                    tmp.Close();
                    texture2D.LoadImage(bytes);
                    texture2D.Compress(true);
                    var rawTextureData = texture2D.GetRawTextureData();
                    var binaryWriter = new BinaryWriter(AllData[ExportArchDataType.Img], Encoding.UTF8, true);
                    binaryWriter.Write(Path.GetFileName(file));
                    binaryWriter.Write(texture2D.width);
                    binaryWriter.Write(texture2D.height);
                    binaryWriter.Write((int) texture2D.format);
                    binaryWriter.Write(rawTextureData.Length);
                    binaryWriter.Write(rawTextureData);
                    binaryWriter.Flush();
                    var maximumOutputSize = LZ4Codec.MaximumOutputLength((int) AllData[ExportArchDataType.Img].Length);
                    if (maximumOutputSize <= MaxBlockSizeForRes) continue;
                    var nativeArray = new byte[maximumOutputSize];
                    new BinaryWriter(AllData[ExportArchDataType.Img], Encoding.UTF8, true).Write(LoadArchMod.EndFlg);
                    var array = AllData[ExportArchDataType.Img].ToArray();
                    var encodeLen = LZ4Codec.Encode(array, 0, array.Length, nativeArray, 0, nativeArray.Length);
                    AllLz4Data[ExportArchDataType.Img].Add((nativeArray, encodeLen, array.Length));
                    AllData[ExportArchDataType.Img].Close();
                    AllData[ExportArchDataType.Img] = new MemoryStream();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            var outputSize = LZ4Codec.MaximumOutputLength((int) AllData[ExportArchDataType.Img].Length);
            var buffer = new byte[outputSize];
            new BinaryWriter(AllData[ExportArchDataType.Img], Encoding.UTF8, true).Write(LoadArchMod.EndFlg);
            var dataBuf = AllData[ExportArchDataType.Img].ToArray();
            var encodeL = LZ4Codec.Encode(dataBuf, 0, dataBuf.Length, buffer, 0, buffer.Length);
            AllLz4Data[ExportArchDataType.Img].Add((buffer, encodeL, dataBuf.Length));
        }

        public void CollectAudio()
        {
            var AudioPath = Path.Combine(ModPath, "Resource", "Audio");
            if (!Directory.Exists(AudioPath)) return;
            foreach (var file in Directory.GetFiles(AudioPath, "*", SearchOption.AllDirectories))
            {
                var fileExtension = Path.GetExtension(file);
                if (!fileExtension.EndsWith("wav", StringComparison.OrdinalIgnoreCase) &&
                    !fileExtension.EndsWith("mp3", StringComparison.OrdinalIgnoreCase) &&
                    !fileExtension.EndsWith("ogg", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var tmp = new MemoryStream();
                    using var audio = File.OpenRead(file);
                    audio.CopyTo(tmp);
                    var bytes = tmp.ToArray();
                    tmp.Close();
                    var binaryWriter = new BinaryWriter(AllData[ExportArchDataType.Audio], Encoding.UTF8, true);
                    binaryWriter.Write(Path.GetFileName(file));
                    binaryWriter.Write(bytes.Length);
                    binaryWriter.Write(bytes);
                    binaryWriter.Flush();
                    var maximumOutputSize =
                        LZ4Codec.MaximumOutputLength((int) AllData[ExportArchDataType.Audio].Length);
                    if (maximumOutputSize <= MaxBlockSizeForRes) continue;
                    var nativeArray = new byte[maximumOutputSize];
                    new BinaryWriter(AllData[ExportArchDataType.Audio], Encoding.UTF8, true).Write(LoadArchMod.EndFlg);
                    var array = AllData[ExportArchDataType.Audio].ToArray();
                    var encodeLen = LZ4Codec.Encode(array, 0, array.Length, nativeArray, 0, nativeArray.Length);
                    AllLz4Data[ExportArchDataType.Audio].Add((nativeArray, encodeLen, array.Length));
                    AllData[ExportArchDataType.Audio].Close();
                    AllData[ExportArchDataType.Audio] = new MemoryStream();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            var outputSize = LZ4Codec.MaximumOutputLength((int) AllData[ExportArchDataType.Audio].Length);
            var buffer = new byte[outputSize];
            new BinaryWriter(AllData[ExportArchDataType.Audio], Encoding.UTF8, true).Write(LoadArchMod.EndFlg);
            var dataBuf = AllData[ExportArchDataType.Audio].ToArray();
            var encodeL = LZ4Codec.Encode(dataBuf, 0, dataBuf.Length, buffer, 0, buffer.Length);
            AllLz4Data[ExportArchDataType.Audio].Add((buffer, encodeL, dataBuf.Length));
        }

        public string GetExt()
        {
            return ModArchVersion switch
            {
                3 => ".modArch_V3",
                2 => ".modArch_V2",
                _ => ".modArch"
            };
        }

        public void CollectAllToPat(string pat)
        {
            if (!Directory.Exists(pat)) return;
            using var memoryStream = CollectAll();
            using var fileStream = File.OpenWrite(Path.Combine(pat, ModName + GetExt()));
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.CopyTo(fileStream);
        }

        public MemoryStream CollectAll()
        {
            switch (ModArchVersion)
            {
                case 1:
                case 2:
                    CollectObj();
                    break;
                case 3:
                    CollectObjV3();
                    break;
            }

            CollectAudio();
            switch (ModArchVersion)
            {
                case 1:
                    CollectImg();
                    break;
                case 2:
                case 3:
                    CollectImgV2();
                    break;
            }

            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, true);
            binaryWriter.Write(ModName);
            binaryWriter.Write("ImgBLK");
            binaryWriter.Write(AllLz4Data[ExportArchDataType.Img].Count);
            for (var i = 0; i < AllLz4Data[ExportArchDataType.Img].Count; i++)
            {
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Img][i].len);
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Img][i].rawLen);
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Img][i].data, 0,
                    AllLz4Data[ExportArchDataType.Img][i].len);
            }

            binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, true);
            binaryWriter.Write("JsonsBLK");
            binaryWriter.Write(AllLz4Data[ExportArchDataType.Jsons].Count);
            for (var i = 0; i < AllLz4Data[ExportArchDataType.Jsons].Count; i++)
            {
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Jsons][i].len);
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Jsons][i].rawLen);
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Jsons][i].data, 0,
                    AllLz4Data[ExportArchDataType.Jsons][i].len);
            }

            binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, true);
            binaryWriter.Write("LocalBLK");
            binaryWriter.Write(AllLz4Data[ExportArchDataType.Local].Count);
            for (var i = 0; i < AllLz4Data[ExportArchDataType.Local].Count; i++)
            {
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Local][i].len);
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Local][i].rawLen);
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Local][i].data, 0,
                    AllLz4Data[ExportArchDataType.Local][i].len);
            }

            binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, true);
            binaryWriter.Write("AudioBLK");
            binaryWriter.Write(AllLz4Data[ExportArchDataType.Audio].Count);
            for (var i = 0; i < AllLz4Data[ExportArchDataType.Audio].Count; i++)
            {
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Audio][i].len);
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Audio][i].rawLen);
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Audio][i].data, 0,
                    AllLz4Data[ExportArchDataType.Audio][i].len);
            }

            binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, true);
            binaryWriter.Write("LuaBLK");
            binaryWriter.Write(AllLz4Data[ExportArchDataType.Lua].Count);
            for (var i = 0; i < AllLz4Data[ExportArchDataType.Lua].Count; i++)
            {
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Lua][i].len);
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Lua][i].rawLen);
                binaryWriter.Write(AllLz4Data[ExportArchDataType.Lua][i].data, 0,
                    AllLz4Data[ExportArchDataType.Lua][i].len);
            }

            binaryWriter.Write(LoadArchMod.EndFlg);

            return memoryStream;
        }

        public enum ExportArchDataType
        {
            Img,
            Jsons,
            Local,
            Audio,
            Lua
        }

        public Dictionary<ExportArchDataType, MemoryStream> AllData = new()
        {
            [ExportArchDataType.Img] = new MemoryStream(),
            [ExportArchDataType.Jsons] = new MemoryStream(),
            [ExportArchDataType.Local] = new MemoryStream(),
            [ExportArchDataType.Lua] = new MemoryStream(),
            [ExportArchDataType.Audio] = new MemoryStream()
        };

        public Dictionary<ExportArchDataType, List<(byte[] data, int len, int rawLen)>> AllLz4Data = new()
        {
            [ExportArchDataType.Img] = new List<(byte[] data, int len, int rawLen)>(),
            [ExportArchDataType.Jsons] = new List<(byte[] data, int len, int rawLen)>(),
            [ExportArchDataType.Local] = new List<(byte[] data, int len, int rawLen)>(),
            [ExportArchDataType.Lua] = new List<(byte[] data, int len, int rawLen)>(),
            [ExportArchDataType.Audio] = new List<(byte[] data, int len, int rawLen)>()
        };

        public ExportArch(string modPath)
        {
            ModPath = modPath;
            ModName = JsonMapper.ToObject(File.ReadAllText(Path.Combine(modPath, "ModInfo.json")))["Name"]
                .ToString();
        }
    }

    public static ExportArch? InitExportArch(string modPath)
    {
        return File.Exists(Path.Combine(modPath, "ModInfo.json")) ? new ExportArch(modPath) : null;
    }
}