using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using HarmonyLib;
using LZ4;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CSTI_MiniLoader.LoadUtil;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
public static class LoadArchMod
{
    public const string EndFlg = "_End_";

    public static void LoadAllArchMod()
    {
        foreach (var file in Directory.EnumerateFiles(MelonHandler.ModsDirectory, "*.modArch_V3",
                     SearchOption.AllDirectories))
        {
            try
            {
                var loadMod = LoadMod(file, 3);
                Debug.Log(loadMod);
            }
            catch (Exception)
            {
            }
        }
    }

    public static string LoadMod(string modPath, int version)
    {
        if (!File.Exists(modPath)) return $"文件 {modPath} 不存在";
        var startTime = DateTime.Now;
        using var fileStream = new BufferedStream(File.OpenRead(modPath), 1024 * 1024);
        using var binaryReader = new BinaryReader(fileStream, Encoding.UTF8, true);
        var modName = binaryReader.ReadString();
        var blk = binaryReader.ReadString();
        while (blk != EndFlg)
        {
            LoadModArchBLK(blk, binaryReader, modName, version);
            blk = binaryReader.ReadString();
        }

        var endTime = DateTime.Now;
        Debug.Log($"加载模组:{modName} 文件总用时:{endTime - startTime:g}");
        return $"加载 {modName} 成功";
    }

    public static void LoadModArchBLK(string blk, BinaryReader reader, string modName, int version)
    {
        if (version <= 2) return;
        switch (blk)
        {
            case "ImgBLK":
                LoadImgBLK_V2(reader, modName);
                break;
            case "AudioBLK":
                LoadAudioBLK(reader, modName);
                break;
            case "LocalBLK":
                LoadLocalBLK(reader, modName);
                break;
            case "JsonsBLK":
                var startTime = DateTime.Now;
                LoadJsonsBLK_V3(reader, modName);
                var endTime = DateTime.Now;
                Debug.Log($"加载模组 {modName} 中的json用时:{endTime - startTime:g}");
                break;
            case "LuaBLK":
                LoadLuaBLK(reader, modName);
                break;
        }
    }

    public static void LoadImgBLK_V2(BinaryReader reader, string modName)
    {
        var dateTime1 = DateTime.Now;
        var blkCount = reader.ReadInt32();
        for (var i = 0; i < blkCount; i++)
        {
            var lz4Len = reader.ReadInt32();
            var blkLen = reader.ReadInt32();
            var bytes = reader.ReadBytes(lz4Len);
            var buffer = new byte[blkLen];
            LZ4Codec.Decode(bytes, 0, bytes.Length, buffer, 0, buffer.Length, true);
            var memoryStream = new MemoryStream(buffer);
            var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8);
            while (true)
            {
                var itemFlg = binaryReader.ReadInt32();
                if (itemFlg == 0) break;
                if (itemFlg == 1)
                {
                    var ImgName = binaryReader.ReadString();
                    var sprite_name = Path.GetFileNameWithoutExtension(ImgName);
                    var width = binaryReader.ReadInt32();
                    var height = binaryReader.ReadInt32();
                    var graphicsFormat = (TextureFormat)binaryReader.ReadInt32();
                    var imgDataLen = binaryReader.ReadInt32();
                    var imgData = binaryReader.ReadBytes(imgDataLen);
                    var t2d = new Texture2D(width, height, graphicsFormat, false);
                    t2d.LoadRawTextureData(imgData);
                    t2d.Apply(false, false);
                    t2d.name = ImgName;
                    var sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height),
                        Vector2.zero);
                    sprite.name = sprite_name;
                    if (!ItemDictionary(typeof(Sprite)).ContainsKey(sprite_name))
                        ItemDictionary(typeof(Sprite)).Add(sprite_name, sprite);
                }
                else if (itemFlg == 2)
                {
                    var rects = binaryReader.ReadRects();
                    var texture2D_name = binaryReader.ReadString();
                    var listStr = binaryReader.ReadListStr();
                    var graphicsFormat = (TextureFormat)binaryReader.ReadInt32();
                    var texPackSizeWidth = binaryReader.ReadInt32();
                    var texPackSizeHeight = binaryReader.ReadInt32();
                    var imgDataLen = binaryReader.ReadInt32();
                    var imgData = binaryReader.ReadBytes(imgDataLen);
                    var t2d = new Texture2D(texPackSizeWidth, texPackSizeHeight, graphicsFormat, false)
                    {
                        name = texture2D_name
                    };
                    t2d.LoadRawTextureData(imgData);
                    t2d.Apply(false, false);
                    for (var j = 0; j < listStr.Count; j++)
                    {
                        var sprite_name = Path.GetFileNameWithoutExtension(listStr[j]);
                        var rect = rects[j];
                        var rect1 = new Rect(rect.x * texPackSizeWidth, rect.y * texPackSizeHeight,
                            rect.width * texPackSizeWidth, rect.height * texPackSizeHeight);
                        var pivot = Vector2.one * 0.5f;
                        var sprite = Sprite.Create(t2d, rect1, pivot, 100, 0, SpriteMeshType.FullRect);
                        sprite.name = sprite_name;
                        if (!ItemDictionary(typeof(Sprite)).ContainsKey(sprite_name))
                            ItemDictionary(typeof(Sprite)).Add(sprite_name, sprite);
                    }
                }
            }
        }

        var dateTime2 = DateTime.Now;
        Debug.Log($"加载模组{modName}中的图片总用时为: {dateTime2 - dateTime1:g}");
    }

    public static void LoadLuaBLK(BinaryReader reader, string modName)
    {
        var blkCount = reader.ReadInt32();
        for (var i = 0; i < blkCount; i++)
        {
            var lz4Len = reader.ReadInt32();
            var blkLen = reader.ReadInt32();
            var bytes = reader.ReadBytes(lz4Len);
            var buffer = new byte[blkLen];
            LZ4Codec.Decode(bytes, 0, bytes.Length, buffer, 0, buffer.Length, true);
            var memoryStream = new MemoryStream(buffer);
            var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8);
            while (true)
            {
                var itemFlg = binaryReader.ReadInt32();
                if (itemFlg == 0) break;
                var listStr = binaryReader.ReadListStr();
                var lua = binaryReader.ReadString();
                if (listStr.Count < 2) continue;
                if (AllLuaFiles.TryGetValue(listStr[0], out var dictionary))
                {
                    dictionary[modName + "_" + string.Join("|", listStr.GetRange(1, listStr.Count - 1))] = lua;
                }
                else
                {
                    AllLuaFiles[listStr[0]] = new Dictionary<string, string>
                    {
                        [modName + "_" + string.Join("|", listStr.GetRange(1, listStr.Count - 1))] = lua
                    };
                }
            }
        }
    }

    public static void LoadJsonsBLK_V3(BinaryReader reader, string modName)
    {
        var allUniqueIDScriptableTypes = (from type in AccessTools.AllTypes()
            where type.IsSubclassOf(typeof(UniqueIDScriptable))
            select type).ToList();
        var allScriptableObjectTypes = (from type in AccessTools.AllTypes()
            where type.IsSubclassOf(typeof(ScriptableObject))
            where !type.IsSubclassOf(typeof(UniqueIDScriptable))
            where type != typeof(UniqueIDScriptable)
            select type).ToList();
        var blkCount = reader.ReadInt32();
        for (var i = 0; i < blkCount; i++)
        {
            var lz4Len = reader.ReadInt32();
            var blkLen = reader.ReadInt32();
            var bytes = reader.ReadBytes(lz4Len);
            var buffer = new byte[blkLen];
            LZ4Codec.Decode(bytes, 0, bytes.Length, buffer, 0, buffer.Length, true);
            var memoryStream = new MemoryStream(buffer);
            var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8);
            var mapper = new StringMapper();
            while (true)
            {
                var itemFlg = binaryReader.ReadInt32();
                if (itemFlg == 0) break;
                if (itemFlg == 2)
                {
                    mapper.Read(binaryReader);
                    continue;
                }

                var listStr = binaryReader.ReadListStr();
                var mapperItem = MapperItem.Read(binaryReader, mapper);
                if (mapperItem == null) continue;
                if (!mapperItem.IsObject) continue;
                var mapperObject = (MapperObject)mapperItem;
                if (listStr.Count == 0) continue;
                if (listStr[0] == "ModInfo.json")
                {
                    // Debug.Log($"正在加载打包模组:{modName}");
                }
                else if (listStr[0] == "ScriptableObject")
                {
                    if (listStr.Count < 3) continue;
                    var obj_name = Path.GetFileNameWithoutExtension(listStr.Last());
                    var find_ScriptableObjectT =
                        allScriptableObjectTypes.FirstOrDefault(type1 => type1.Name == listStr[1]);
                    var dict = ItemDictionary(find_ScriptableObjectT);
                    if (dict.ContainsKey(obj_name))
                        continue;

                    var obj = ScriptableObject.CreateInstance(Il2CppType.From(find_ScriptableObjectT));

                    obj.name = obj_name;
                    try
                    {
                        JsonUtility.FromJsonOverwrite(mapperObject.ToJson(), obj);
                    }
                    catch (Exception)
                    {
                    }

                    if (!dict.ContainsKey(obj_name))
                        dict.Add(obj_name, obj);
                    WaitForWarpperEditorNoGuidList.Add(new ScriptableObjectPack(obj,
                        "", "", modName, mapperObject));
                    RegObj(obj_name, obj, find_ScriptableObjectT);
                }
                else if (listStr[0] == "GameSourceModify")
                {
                    var Guid = Path.GetFileNameWithoutExtension(listStr.Last());

                    WaitForWarpperEditorGameSourceGUIDList.Add(
                        AllGUIDDict.TryGetValue(Guid, out var obj)
                            ? new ScriptableObjectPack(obj, "", "", modName, mapperObject)
                            : new ScriptableObjectPack(null, Guid, "", modName, mapperObject));
                }
                else
                {
                    var type = allUniqueIDScriptableTypes.FirstOrDefault(type => type.Name == listStr[0]);
                    if (type == null) continue;
                    var CardName = Path.GetFileNameWithoutExtension(listStr.Last());
                    try
                    {
                        if (!(mapperObject.ContainsKey("UniqueID") && mapperObject["UniqueID"].IsString &&
                              !string.IsNullOrEmpty(mapperObject["UniqueID"].ToString())))
                        {
                            continue;
                        }

                        var card = ScriptableObject.CreateInstance(Il2CppType.From(type)) as UniqueIDScriptable;
                        // JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                        try
                        {
                            JsonUtility.FromJsonOverwrite(mapperObject.ToJson(), card);
                        }
                        catch (Exception)
                        {
                        }

                        card!.name = $"{modName}_{CardName}";

                        //type.GetMethod("Init", bindingFlags, null, new Type[] { }, null).Invoke(card, null);
                        var card_guid = card.UniqueID;
                        if (!AllGUIDDict.ContainsKey(card_guid))
                        {
                            AllGUIDDict.Add(card_guid, card);
                            GameLoad.Instance.DataBase.AllData.Add(card);
                        }

                        if (!WaitForWarpperEditorGuidDict.ContainsKey(card_guid))
                            WaitForWarpperEditorGuidDict.Add(card_guid,
                                new ScriptableObjectPack(card, "", "", modName,
                                    mapperObject));
                        RegObj(card_guid, card, type);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }

    public static void LoadLocalBLK(BinaryReader reader, string modName)
    {
        var blkCount = reader.ReadInt32();
        for (var i = 0; i < blkCount; i++)
        {
            var lz4Len = reader.ReadInt32();
            var blkLen = reader.ReadInt32();
            var bytes = reader.ReadBytes(lz4Len);
            var buffer = new byte[blkLen];
            LZ4Codec.Decode(bytes, 0, bytes.Length, buffer, 0, buffer.Length, true);
            var memoryStream = new MemoryStream(buffer);
            var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8);
            while (true)
            {
                var LocalName = binaryReader.ReadString();
                if (LocalName == EndFlg) break;
                var LocalContent = binaryReader.ReadString();
                WaitForLoadCSVList.Add((LocalName, LocalContent));
            }
        }
    }

    public static void LoadAudioBLK(BinaryReader reader, string modName)
    {
        var blkCount = reader.ReadInt32();
        for (var i = 0; i < blkCount; i++)
        {
            var lz4Len = reader.ReadInt32();
            var blkLen = reader.ReadInt32();
            var bytes = reader.ReadBytes(lz4Len);
            var buffer = new byte[blkLen];
            LZ4Codec.Decode(bytes, 0, bytes.Length, buffer, 0, buffer.Length, true);
            var memoryStream = new MemoryStream(buffer);
            var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8);
            while (true)
            {
                var AudioName = binaryReader.ReadString();
                if (AudioName == EndFlg)
                {
                    break;
                }

                var stream = new MemoryStream();
                var audioDataLen = binaryReader.ReadInt32();
                var audioData = binaryReader.ReadBytes(audioDataLen);
                stream.Write(audioData, 0, audioDataLen);
                stream.Seek(0, SeekOrigin.Begin);
                if (AudioName.EndsWith(".wav", true, null))
                {
                    var clip_name = Path.GetFileNameWithoutExtension(AudioName);
                    var clip = ResourceDataLoader.GetAudioClipFromWav(stream, clip_name);
                    if (!clip) continue;
                    RegObj(clip_name, clip, typeof(AudioClip));
                }
                else if (AudioName.EndsWith(".mp3", true, null))
                {
                    var clip_name = Path.GetFileNameWithoutExtension(AudioName);
                    var clip = ResourceDataLoader.GetAudioClipFromMp3(stream, clip_name);
                    if (!clip) continue;
                    RegObj(clip_name, clip, typeof(AudioClip));
                }
            }
        }
    }
}