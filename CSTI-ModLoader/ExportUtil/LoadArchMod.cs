using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using K4os.Compression.LZ4;
using LitJson;
using ModLoader.LoaderUtil;
using UnityEngine;

namespace ModLoader.ExportUtil;

public static class LoadArchMod
{
    public const string EndFlg = "_End_";

    private delegate Sprite CreateSpriteWithoutTextureScripting_InjectedDelegate(ref Rect rect, ref Vector2 pivot,
        float pixelsToUnits, Texture2D texture);

    private static CreateSpriteWithoutTextureScripting_InjectedDelegate CreateSpriteWithoutTextureScripting_Injected =
        (CreateSpriteWithoutTextureScripting_InjectedDelegate) AccessTools.DeclaredMethod(typeof(Sprite),
                nameof(CreateSpriteWithoutTextureScripting_Injected))
            .CreateDelegate(typeof(CreateSpriteWithoutTextureScripting_InjectedDelegate));

    public static void LoadAllArchMod()
    {
        foreach (var file in Directory.EnumerateFiles(Paths.PluginPath, "*.modArch_V2", SearchOption.AllDirectories))
        {
            try
            {
                var loadMod = LoadMod(file, 2);
                if (loadMod == null) continue;
                Debug.Log(loadMod);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        foreach (var file in Directory.EnumerateFiles(Paths.PluginPath, "*.modArch", SearchOption.AllDirectories))
        {
            try
            {
                var loadMod = LoadMod(file, 1);
                if (loadMod == null) continue;
                Debug.Log(loadMod);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }

    [CanBeNull]
    public static string LoadMod(string modPath, int version)
    {
        if (!File.Exists(modPath)) return $"文件 {modPath} 不存在";
        using var fileStream = File.OpenRead(modPath);
        using var binaryReader = new BinaryReader(fileStream, Encoding.UTF8);
        var modName = binaryReader.ReadString();
        if (ModPacks.TryGetValue(modName, out var pack))
        {
            if (!pack.EnableEntry.Value) return $"{modName} 模组被禁用";
            if (pack.Loaded) return null;
            pack.Loaded = true;
        }
        else
        {
            var modInfo = new ModInfo
                {Name = modName, ModLoaderVerison = ModVersion, ModEditorVersion = "", Version = "0.0.0"};
            ModPacks[modName] = new ModPack(modInfo, modName, ModLoaderInstance.Config.Bind("是否加载某个模组",
                $"{Path.GetFileNameWithoutExtension(modPath)}_{modName}".EscapeStr(), true,
                $"是否加载{modName}"), true);
        }

        var blk = binaryReader.ReadString();
        while (blk != EndFlg)
        {
            LoadModArchBLK(blk, binaryReader, modName, version);
            blk = binaryReader.ReadString();
        }

        return $"加载 {modName} 成功";
    }

    public static void LoadModArchBLK(string blk, BinaryReader reader, string modName, int version)
    {
        switch (blk)
        {
            case "ImgBLK" when version == 2:
                LoadImgBLK_V2(reader, modName);
                break;
            case "ImgBLK":
                LoadImgBLK(reader, modName);
                break;
            case "AudioBLK":
                LoadAudioBLK(reader, modName);
                break;
            case "LocalBLK":
                LoadLocalBLK(reader, modName);
                break;
            case "JsonsBLK":
                LoadJsonsBLK(reader, modName);
                break;
            case "LuaBLK":
                LoadLuaBLK(reader, modName);
                break;
            default:
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
            LZ4Codec.Decode(bytes, buffer);
            var memoryStream = new MemoryStream(buffer);
            buffer = null;
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
                    var graphicsFormat = (TextureFormat) binaryReader.ReadInt32();
                    var imgDataLen = binaryReader.ReadInt32();
                    var imgData = binaryReader.ReadBytes(imgDataLen);
                    var t2d = new Texture2D(width, height, graphicsFormat, -1, false);
                    t2d.LoadRawTextureData(imgData);
                    t2d.Apply(false, false);
                    t2d.name = ImgName;
                    var sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height),
                        Vector2.zero);
                    sprite.name = sprite_name;
                    if (!SpriteDict.ContainsKey(sprite_name))
                        SpriteDict.Add(sprite_name, sprite);
                    else
                        Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}", modName,
                            sprite_name);
                }
                else if (itemFlg == 2)
                {
                    var rects = binaryReader.ReadRects();
                    var texture2D_name = binaryReader.ReadString();
                    var listStr = binaryReader.ReadListStr();
                    var graphicsFormat = (TextureFormat) binaryReader.ReadInt32();
                    var texPackSize = binaryReader.ReadInt32();
                    var imgDataLen = binaryReader.ReadInt32();
                    var imgData = binaryReader.ReadBytes(imgDataLen);
                    var t2d = new Texture2D(texPackSize, texPackSize, graphicsFormat, -1, false)
                    {
                        name = texture2D_name
                    };
                    t2d.LoadRawTextureData(imgData);
                    t2d.Apply(false, false);
                    for (var j = 0; j < listStr.Count; j++)
                    {
                        var sprite_name = Path.GetFileNameWithoutExtension(listStr[j]);
                        var rect = rects[j];
                        var rect1 = new Rect(rect.x * texPackSize, rect.y * texPackSize, rect.width * texPackSize,
                            rect.height * texPackSize);
                        var pivot = Vector2.one * 0.5f;
                        var sprite = Sprite.Create(t2d, rect1, pivot, 100, 0, SpriteMeshType.FullRect);
                        sprite.name = sprite_name;
                        if (!SpriteDict.ContainsKey(sprite_name))
                            SpriteDict.Add(sprite_name, sprite);
                        else
                            Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}", modName,
                                sprite_name);
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
            LZ4Codec.Decode(bytes, buffer);
            var memoryStream = new MemoryStream(buffer);
            buffer = null;
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

    public static void LoadJsonsBLK(BinaryReader reader, string modName)
    {
        var allUniqueIDScriptableTypes = (from type in AccessTools.AllTypes()
            where type.IsSubclassOf(typeof(UniqueIDScriptable))
            select type).ToList();
        var blkCount = reader.ReadInt32();
        for (var i = 0; i < blkCount; i++)
        {
            var lz4Len = reader.ReadInt32();
            var blkLen = reader.ReadInt32();
            var bytes = reader.ReadBytes(lz4Len);
            var buffer = new byte[blkLen];
            LZ4Codec.Decode(bytes, buffer);
            var memoryStream = new MemoryStream(buffer);
            buffer = null;
            var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8);
            while (true)
            {
                var itemFlg = binaryReader.ReadInt32();
                if (itemFlg == 0) break;
                var listStr = binaryReader.ReadListStr();
                var json = binaryReader.ReadString();
                if (listStr.Count == 0) continue;
                if (listStr[0] == "ModInfo.json")
                {
                    Debug.Log($"正在加载打包模组:{modName}");
                }
                else if (listStr[0] == "ScriptableObject")
                {
                    if (listStr.Count < 1) continue;
                    var obj_name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(listStr.Last()));
                    var (type, dict) =
                        AllScriptableObjectWithoutGuidTypeDict.FirstOrDefault(pair => pair.Key.Name == listStr[1]);
                    if (dict == null) continue;
                    if (dict.ContainsKey(obj_name))
                        continue;

                    var obj = ScriptableObject.CreateInstance(type);

                    obj.name = obj_name;
                    JsonUtility.FromJsonOverwrite(json, obj);
                    if (!dict.ContainsKey(obj_name))
                        dict.Add(obj_name, obj);
                    WaitForWarpperEditorNoGuidList.Add(new ScriptableObjectPack(obj,
                        "", "", modName, json));
                    if (!AllScriptableObjectDict.ContainsKey(obj_name))
                        AllScriptableObjectDict.Add(obj_name, obj);
                }
                else if (listStr[0] == "GameSourceModify")
                {
                    var Guid = Path.GetFileNameWithoutExtension(listStr.Last());

                    WaitForWarpperEditorGameSourceGUIDList.Add(
                        AllGUIDDict.TryGetValue(Guid, out var obj)
                            ? new ScriptableObjectPack(obj, "", "", modName, json)
                            : new ScriptableObjectPack(null, Guid, "", modName, json));
                }
                else
                {
                    var type = allUniqueIDScriptableTypes.FirstOrDefault(type => type.Name == listStr[0]);
                    if (type == null) continue;
                    var CardName = Path.GetFileNameWithoutExtension(listStr.Last());
                    try
                    {
                        var jsonData = JsonMapper.ToObject(json);

                        if (!(jsonData.ContainsKey("UniqueID") && jsonData["UniqueID"].IsString &&
                              !jsonData["UniqueID"].ToString().IsNullOrWhiteSpace()))
                        {
                            Debug.LogErrorFormat(
                                "{0} EditorLoadZip {1} {2} try to load a UniqueIDScriptable without GUID",
                                type.Name, modName, CardName);
                            continue;
                        }

                        var card = ScriptableObject.CreateInstance(type) as UniqueIDScriptable;
                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                        JsonUtility.FromJsonOverwrite(json, card);

                        card.name = $"{modName}_{CardName}";

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
                                    json));
                        else
                            Debug.LogWarningFormat(
                                "{0} WaitForWarpperEditorGuidDict Same Key was Add {1}", modName,
                                card_guid);
                        if (!AllScriptableObjectDict.ContainsKey(card_guid))
                            AllScriptableObjectDict.Add(card_guid, card);
                        if (AllGUIDTypeDict.TryGetValue(type, out var dict))
                            if (!dict.ContainsKey(card_guid))
                                dict.Add(card_guid, card);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarningFormat("{0} EditorLoadZip {1} {2} Error {3}", type.Name,
                            modName, CardName, ex.Message);
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
            LZ4Codec.Decode(bytes, buffer);
            var memoryStream = new MemoryStream(buffer);
            buffer = null;
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

    public static void LoadImgBLK(BinaryReader reader, string modName)
    {
        var blkCount = reader.ReadInt32();
        for (var i = 0; i < blkCount; i++)
        {
            var lz4Len = reader.ReadInt32();
            var blkLen = reader.ReadInt32();
            var bytes = reader.ReadBytes(lz4Len);
            var buffer = new byte[blkLen];
            LZ4Codec.Decode(bytes, buffer);
            var memoryStream = new MemoryStream(buffer);
            buffer = null;
            var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8);
            while (true)
            {
                var ImgName = binaryReader.ReadString();
                if (ImgName == EndFlg)
                {
                    break;
                }

                var width = binaryReader.ReadInt32();
                var height = binaryReader.ReadInt32();
                var graphicsFormat = (TextureFormat) binaryReader.ReadInt32();
                var imgDataLen = binaryReader.ReadInt32();
                var imgData = binaryReader.ReadBytes(imgDataLen);
                var sprite_name = Path.GetFileNameWithoutExtension(ImgName);
                var t2d = new Texture2D(width, height, graphicsFormat, -1, false);
                t2d.LoadRawTextureData(imgData);
                t2d.Apply(false, false);
                var sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height),
                    Vector2.zero, 100, 0, SpriteMeshType.FullRect);
                sprite.name = sprite_name;
                if (!SpriteDict.ContainsKey(sprite_name))
                    SpriteDict.Add(sprite_name, sprite);
                else
                    Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}", modName,
                        sprite_name);
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
            LZ4Codec.Decode(bytes, buffer);
            var memoryStream = new MemoryStream(buffer);
            buffer = null;
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
                audioData = null;
                if (AudioName.EndsWith(".wav", true, null))
                {
                    var clip_name = Path.GetFileNameWithoutExtension(AudioName);
                    var clip = ResourceDataLoader.GetAudioClipFromWav(stream, clip_name);
                    if (!clip) continue;
                    if (!AudioClipDict.ContainsKey(clip.name))
                        AudioClipDict.Add(clip.name, clip);
                    else
                        Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}",
                            modName, clip.name);
                }
                else if (AudioName.EndsWith(".mp3", true, null))
                {
                    var clip_name = Path.GetFileNameWithoutExtension(AudioName);
                    var clip = ResourceDataLoader.GetAudioClipFromMp3(stream, clip_name);
                    if (!clip) continue;
                    if (!AudioClipDict.ContainsKey(clip.name))
                        AudioClipDict.Add(clip.name, clip);
                    else
                        Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}",
                            modName, clip.name);
                }
                else if (AudioName.EndsWith(".ogg", true, null))
                {
                    var clip_name = Path.GetFileNameWithoutExtension(AudioName);
                    var clip = ResourceDataLoader.GetAudioClipFromOgg(stream, clip_name);
                    if (!clip) continue;
                    if (!AudioClipDict.ContainsKey(clip.name))
                        AudioClipDict.Add(clip.name, clip);
                    else
                        Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}",
                            modName, clip.name);
                }
            }
        }
    }
}