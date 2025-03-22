using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CSTI_MiniLoader.LoadUtil;
using CSTI_MiniLoader.Patchers;
using HarmonyLib;
using LitJson;
using MelonLoader;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Networking;

namespace CSTI_MiniLoader;

public class MiniLoader : MelonMod
{
    public struct ScriptableObjectPack
    {
        public ScriptableObject? Obj;
        public readonly string CardDirOrGuid;
        public string CardPath;
        public readonly string ModName;
        public readonly KVProvider? CardData;

        public ScriptableObjectPack(ScriptableObject? obj,
            string cardDirOrGuid,
            string cardPath,
            string modName,
            KVProvider? cardData)
        {
            Obj = obj;
            CardDirOrGuid = cardDirOrGuid;
            CardPath = cardPath;
            ModName = modName;
            CardData = cardData;
        }
    }

    public struct CSVItem
    {
        public string LocalName;
        public string LocalContent;

        public CSVItem(string localName, string localContent)
        {
            LocalName = localName;
            LocalContent = localContent;
        }
    }

    public const string Version = "0.0.2";
    public static readonly Dictionary<Type, Dictionary<string, object>> AllItemDictionary = new();
    public static readonly Dictionary<string, Dictionary<string, string>> AllLuaFiles = new();
    public static readonly List<CSVItem> WaitForLoadCSVList = new();
    public static readonly Dictionary<string, UniqueIDScriptable> AllGUIDDict = new();
    public static readonly Dictionary<string, ScriptableObject> AllScriptableObjectDict = new();
    public static readonly List<ScriptableObjectPack> WaitForWarpperEditorNoGuidList = new();
    public static readonly List<ScriptableObjectPack> WaitForWarpperEditorGameSourceGUIDList = new();
    public static readonly Dictionary<string, ScriptableObjectPack> WaitForWarpperEditorGuidDict = new();
    public static readonly List<ScriptableObjectPack> WaitForAddCardTabGroup = new();
    public static readonly List<ScriptableObjectPack> WaitForAddJournalPlayerCharacter = new();
    public static readonly List<ScriptableObjectPack> WaitForAddDefaultContentPage = new();
    public static readonly List<ScriptableObjectPack> WaitForAddMainContentPage = new();
    public static readonly List<GuideEntry> WaitForAddGuideEntry = new();
    public static readonly List<Tuple<string, string, CardData>> WaitForAddBlueprintCard = new();
    public static readonly List<Tuple<string, CardData>> WaitForAddCardFilterGroupCard = new();
    public static readonly List<Tuple<string, CharacterPerk>> WaitForAddPerkGroup = new();
    public static readonly List<Tuple<string, GameStat>> WaitForAddVisibleGameStat = new();
    public static readonly List<ScriptableObjectPack> WaitForMatchAndWarpperEditorGameSourceList = new();
    public static readonly Dictionary<string, Dictionary<string, CardData>> AllCardTagGuidCardDataDict = new();
    public static readonly Dictionary<string, GameObject> CustomGameObjectListDict = new();
    public static readonly Dictionary<string, ContentDisplayer> CustomContentDisplayerDict = new();
    public static readonly HarmonyLib.Harmony HarmonyIns = new("zender.CSTI-MiniLoader");

    public static void RegObj(string id, object o, Type? type)
    {
        if (ItemDictionary(type).ContainsKey(id)) return;
        ItemDictionary(type)[id] = o;
        if (type != null && type.IsSubclassOf(typeof(UniqueIDScriptable)))
        {
            AllGUIDDict[id] = (UniqueIDScriptable)o;
        }

        if (type != null && type.IsSubclassOf(typeof(ScriptableObject)))
        {
            AllScriptableObjectDict[id] = (ScriptableObject)o;
        }
    }

    public static Dictionary<string, object> ItemDictionary(Type? type)
    {
        if (type == null)
        {
            return new Dictionary<string, object>();
        }

        if (AllItemDictionary.TryGetValue(type, out var dictionary))
        {
            return dictionary;
        }

        var objects = new Dictionary<string, object>();
        AllItemDictionary[type] = objects;
        return objects;
    }

    public override void OnInitializeMelon()
    {
        var modPath = Path.GetDirectoryName(MelonAssembly.Location)!;
        MelonCoroutines.Start(LoadPictures(modPath));
        LoadMods(modPath);
        HarmonyIns.PatchAll(typeof(LoadPatchMain));
        MelonLogger.Msg($"ModPath:{Path.GetDirectoryName(MelonAssembly.Location)}");
    }

    private static IEnumerator LoadPictures(string modPath)
    {
        LoadPatchMain.CanInit = true;
        yield break;
        var dirs = Directory.GetDirectories(modPath);
        foreach (var dir in dirs)
        {
            // Load Resource Custom Picture
            if (Directory.Exists(Path.Combine(dir, "Resource", "Picture")))
            {
                var files = Directory.GetFiles(Path.Combine(dir, "Resource", "Picture"));
                foreach (var file in files)
                {
                    var request = UnityWebRequestTexture.GetTexture("file://" + file);
                    yield return request.SendWebRequest();

                    var t2d = DownloadHandlerTexture.GetContent(request);
                    var sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.one * 0.5f);
                    sprite.name = Path.GetFileNameWithoutExtension(file);
                    RegObj(sprite.name, sprite, sprite.GetType());
                }
            }
        }

        LoadPatchMain.CanInit = true;
    }

    private static void LoadMods(string modPath)
    {
        try
        {
            var dirs = Directory.GetDirectories(modPath);
            foreach (var dir in dirs)
            {
                //  Check if is a Mod Directory
                if (!File.Exists(Path.Combine(dir, "ModInfo.json")))
                    continue;

                var Info = new ModInfo();
                var ModName = Path.GetFileName(dir);

                try
                {
                    // Load Mod Info
                    using (var sr = new StreamReader(Path.Combine(dir, "ModInfo.json")))
                    {
                        var jsonData = JsonMapper.ToObject(sr.ReadToEnd());
                        Info.Name = jsonData[nameof(ModInfo.Name)].ToString();
                        Info.ModEditorVersion = jsonData[nameof(ModInfo.ModEditorVersion)].ToString();
                        Info.Version = jsonData[nameof(ModInfo.Version)].ToString();
                        Info.ModLoaderVerison = jsonData[nameof(ModInfo.ModLoaderVerison)].ToString();
                    }

                    // Check Name
                    if (!string.IsNullOrWhiteSpace(Info.Name))
                        ModName = Info.Name;

                    MelonLogger.Msg($"ModLoader Load Mod {ModName} {Info.Version}");
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"{ModName} Check Version Error {ex.Message}");
                }

                // Load Resource Custom Audio
                // try
                // {
                //     if (Directory.Exists(Path.Combine(dir, "Resource", "Audio")))
                //     {
                //         var files = Directory.GetFiles(Path.Combine(dir, "Resource", "Audio"));
                //         foreach (var file in files)
                //             if (file.EndsWith(".wav", true, null))
                //             {
                //                 var raw_data = File.Open(file, FileMode.Open);
                //                 var clip_name = Path.GetFileNameWithoutExtension(file);
                //                 var clip = ResourceDataLoader.GetAudioClipFromWav(raw_data, clip_name);
                //                 if (!clip) continue;
                //                 RegObj(clip_name, clip, clip.GetType());
                //             }
                //             else if (file.EndsWith(".mp3", true, null))
                //             {
                //                 var raw_data = File.Open(file, FileMode.Open);
                //                 var clip_name = Path.GetFileNameWithoutExtension(file);
                //                 var clip = ResourceDataLoader.GetAudioClipFromMp3(raw_data, clip_name);
                //                 if (!clip) continue;
                //                 RegObj(clip_name, clip, clip.GetType());
                //             }
                //         //MBSingleton<GameLoad>.Instance.StartCoroutine(GetDataRequest(ModName, file));
                //     }
                // }
                // catch (Exception ex)
                // {
                //     MelonLogger.Warning($"{ModName} Load Resource Custom Audio Error {ex.Message}");
                // }

                // Load ScriptableObject
                try
                {
                    if (Directory.Exists(Path.Combine(dir, "ScriptableObject")))
                    {
                        var subclasses = from type in AccessTools.AllTypes()
                            where type.IsSubclassOf(typeof(ScriptableObject))
                            select type;

                        foreach (var type in subclasses)
                        {
                            if (type.IsSubclassOf(typeof(UniqueIDScriptable)) || type == typeof(UniqueIDScriptable))
                                continue;

                            if (!Directory.Exists(Path.Combine(dir, "ScriptableObject", type.Name)))
                                continue;

                            foreach (var file in Directory.EnumerateFiles(
                                         Path.Combine(dir, "ScriptableObject", type.Name), "*.json",
                                         SearchOption.AllDirectories))
                            {
                                var obj_name = Path.GetFileNameWithoutExtension(file);
                                string CardData;

                                var obj = ScriptableObject.CreateInstance(Il2CppType.From(type));
                                using (var sr = new StreamReader(file))
                                {
                                    CardData = sr.ReadToEnd();
                                }

                                obj.name = obj_name;
                                var jsonData = JsonMapper.ToObject(CardData);
                                // if (obj is IModLoaderJsonObj modLoaderJsonObj)
                                // {
                                //     modLoaderJsonObj.CreateByJson(CardData);
                                // }
                                // else
                                {
                                    JsonUtility.FromJsonOverwrite(CardData, obj);
                                }

                                RegObj(obj_name, obj, obj.GetType());
                                WaitForWarpperEditorNoGuidList.Add(
                                    new ScriptableObjectPack(obj, "", "", ModName,
                                        new JsonKVProvider(jsonData)));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"{ModName} Load ScriptableObject Error {ex.Message}");
                }

                // Load Localization
                try
                {
                    if (Directory.Exists(Path.Combine(dir, "Localization")))
                    {
                        var files = Directory.GetFiles(Path.Combine(dir, "Localization"));
                        foreach (var file in files)
                        {
                            if (!file.EndsWith(".csv"))
                                continue;
                            using (var sr = new StreamReader(file))
                            {
                                WaitForLoadCSVList.Add(new CSVItem(Path.GetFileName(file), sr.ReadToEnd()));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"{ModName} Load Localization Error {ex.Message}");
                }

                // Load and init UniqueIDScriptable
                try
                {
                    foreach (var type in Directory.EnumerateDirectories(dir))
                    {
                        var type_name = Path.GetFileName(type);
                        var t = AccessTools.TypeByName(type_name);
                        if (t == null) continue;
                        foreach (var file in Directory.EnumerateFiles(type, "*.json",
                                     SearchOption.AllDirectories))
                        {
                            string CardName = Path.GetFileNameWithoutExtension(file);
                            try
                            {
                                JsonData json;
                                var CardData = "";
                                using (StreamReader sr = new StreamReader(file))
                                {
                                    CardData = sr.ReadToEnd();
                                    json = JsonMapper.ToObject(CardData);
                                }

                                if (!(json.ContainsKey("UniqueID") && json["UniqueID"].IsString &&
                                      !string.IsNullOrWhiteSpace(json["UniqueID"].ToString())))
                                {
                                    continue;
                                }

                                var bindingFlags = (BindingFlags)(-1);
                                var card = ScriptableObject.CreateInstance(Il2CppType.From(t))
                                    .Cast<UniqueIDScriptable>();
                                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                                JsonUtility.FromJsonOverwrite(CardData, card);

                                card.name = ModName + "_" + CardName;

                                var card_guid = card.UniqueID;
                                AllGUIDDict.Add(card_guid, card);
                                GameLoad.Instance.DataBase.AllData.Add(card);

                                if (!WaitForWarpperEditorGuidDict.ContainsKey(card_guid))
                                    WaitForWarpperEditorGuidDict.Add(card_guid,
                                        new ScriptableObjectPack(card, "", file,
                                            ModName, new JsonKVProvider(CardData)));
                                RegObj(card_guid, card, t);
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Warning($"{type_name} EditorLoad {ModName} {CardName} Error {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"{ModName} Load UniqueIDScriptable Error {ex.Message}");
                }

                // Load GameSourceModify
                try
                {
                    if (Directory.Exists(Path.Combine(dir, "GameSourceModify")))
                        foreach (var file in Directory.EnumerateFiles(Path.Combine(dir, "GameSourceModify"),
                                     "*.json", SearchOption.AllDirectories))
                        {
                            var CardPath = file;
                            string CardData;
                            var Guid = Path.GetFileNameWithoutExtension(file);
                            using (var sr = new StreamReader(CardPath))
                            {
                                CardData = sr.ReadToEnd();
                            }

                            var jsonData = JsonMapper.ToObject(CardData);
                            WaitForWarpperEditorGameSourceGUIDList.Add(
                                AllGUIDDict.TryGetValue(Guid, out var obj)
                                    ? new ScriptableObjectPack(obj, "", "", ModName, new JsonKVProvider(jsonData))
                                    : new ScriptableObjectPack(null, Guid, "", ModName,
                                        new JsonKVProvider(jsonData)));
                        }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"{ModName} Load GameSourceModify Error {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Warning(ex.Message);
        }
    }
}

public class ModInfo
{
    public string Name = "";
    public string Version = "";

    // ReSharper disable once IdentifierTypo
    public string ModLoaderVerison = "";

    public string ModEditorVersion = "";
}