using System;
using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Text;
using LitJson;
using System.Linq;
using Ionic.Zip;
using System.Collections;
using System.Security.Cryptography;

namespace ModLoader
{

    public class ModInfo
    {
        public string Name;
        public string Version;
        public string ModLoaderVerison;
        public string ModEditorVersion;
    }

    [BepInPlugin("Dop.plugin.CSTI.ModLoader", "ModLoader", "1.3.1")]
    public class ModLoader : BaseUnityPlugin
    {
        public static System.Version PluginVersion;
        public static Assembly GameSrouceAssembly;

        public static Dictionary<string, UnityEngine.Sprite> SpriteDict = new Dictionary<string, UnityEngine.Sprite>();
        public static Dictionary<string, UnityEngine.AudioClip> AudioClipDict = new Dictionary<string, UnityEngine.AudioClip>();
        public static Dictionary<string, WeatherSpecialEffect> WeatherSpecialEffectDict = new Dictionary<string, WeatherSpecialEffect>();

        // GUID Dict
        public static Dictionary<string, UniqueIDScriptable> AllGUIDDict = new Dictionary<string, UniqueIDScriptable>();
        public static Dictionary<Type, Dictionary<string, UniqueIDScriptable>> AllGUIDTypeDict = new Dictionary<Type, Dictionary<string, UniqueIDScriptable>>();
        // AllCardTagGuidCardDataDict[CardTag.name][Guid] = CardData
        public static Dictionary<string, Dictionary<string, CardData>> AllCardTagGuidCardDataDict = new Dictionary<string, Dictionary<string, CardData>>();

        // will deprecated in future version 
        public static Dictionary<string, CardTag> CardTagDict = new Dictionary<string, CardTag>();
        public static Dictionary<string, EquipmentTag> EquipmentTagDict = new Dictionary<string, EquipmentTag>();
        public static Dictionary<string, ActionTag> ActionTagDict = new Dictionary<string, ActionTag>();
        public static Dictionary<string, CardTabGroup> CardTabGroupDict = new Dictionary<string, CardTabGroup>();
        public static Dictionary<string, WeatherSet> WeatherSetDict = new Dictionary<string, WeatherSet>();

        // ScriptableObject Dict
        // AllScriptableObjectDict[GUID or name] = ScriptableObject
        public static Dictionary<string, ScriptableObject> AllScriptableObjectDict = new Dictionary<string, ScriptableObject>();
        public static Dictionary<Type, Dictionary<string, ScriptableObject>> AllScriptableObjectWithoutGuidTypeDict = new Dictionary<Type, Dictionary<string, ScriptableObject>>();
        public static Dictionary<string, Type> ScriptableObjectKeyType = new Dictionary<string, Type>();

        // Special Dict(Vulnerable Function)
        public static Dictionary<string, ContentDisplayer> CustomContentDisplayerDict = new Dictionary<string, ContentDisplayer>();
        public static Dictionary<string, GameObject> CustomGameObjectListDict = new Dictionary<string, GameObject>();

        public struct ScriptableObjectPack
        {
            public ScriptableObjectPack(ScriptableObject obj, string CardDir, string CardPath, string ModName, string CardData = "")
            {
                this.obj = obj;
                this.CardDir = CardDir;
                this.CardPath = CardPath;
                this.ModName = ModName;
                this.CardData = CardData;
            }
            public ScriptableObject obj;
            public string CardDir;
            public string CardPath;
            public string ModName;
            public string CardData;
        }

        public static ScriptableObjectPack ProcessingScriptableObjectPack = new ScriptableObjectPack();

        private static Dictionary<string, ScriptableObjectPack> WaitForWarpperGuidDict = new Dictionary<string, ScriptableObjectPack>();
        private static Dictionary<string, ScriptableObjectPack> WaitForWarpperEditorGuidDict = new Dictionary<string, ScriptableObjectPack>();

        private static List<ScriptableObjectPack> WaitForWarpperEditorNoGuidlist = new List<ScriptableObjectPack>();
        private static List<ScriptableObjectPack> WaitForWarpperGameSourceGUIDList = new List<ScriptableObjectPack>();
        private static List<ScriptableObjectPack> WaitForWarpperEditorGameSourceGUIDList = new List<ScriptableObjectPack>();
        private static List<ScriptableObjectPack> WaitForMatchAndWarpperEditorGameSourceList = new List<ScriptableObjectPack>();

        private static List<Tuple<string, string>> WaitForLoadCSVList = new List<Tuple<string, string>>();
        private static List<Tuple<string, string, CardData>> WaitForAddBlueprintCard = new List<Tuple<string, string, CardData>>();
        private static List<Tuple<string, GameStat>> WaitForAddVisibleGameStat = new List<Tuple<string, GameStat>>();
        private static List<Tuple<string, CharacterPerk>> WaitForAddPerkGroup = new List<Tuple<string, CharacterPerk>>();

        private static List<ScriptableObjectPack> WaitForAddCardTabGroup = new List<ScriptableObjectPack>();
        private static List<ScriptableObjectPack> WaitForAddJournalPlayerCharacter = new List<ScriptableObjectPack>();
        private static List<ScriptableObjectPack> WaitForAddDefaultContentPage = new List<ScriptableObjectPack>();
        private static List<ScriptableObjectPack> WaitForAddMainContentPage = new List<ScriptableObjectPack>();

        private void Awake()
        {
            // Plugin startup logic
            Harmony.CreateAndPatchAll(typeof(ModLoader), this.Info.Metadata.GUID);
            PluginVersion = System.Version.Parse(this.Info.Metadata.Version.ToString());
            Logger.LogInfo("Plugin ModLoader is loaded! ");
        }

        public static string CombinePaths(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            return paths.Aggregate(Path.Combine);
        }

        public static bool IsSubDirectory(string dir, string parent_dir)
        {
            DirectoryInfo di1 = new DirectoryInfo(parent_dir);
            DirectoryInfo di2 = new DirectoryInfo(dir);
            bool isParent = false;
            while (di2.Parent != null)
            {
                if (di2.Parent.FullName == di1.FullName)
                {
                    isParent = true;
                    break;
                }
                else di2 = di2.Parent;
            }
            return isParent;
        }

        public static void LogErrorWithModInfo(string error_info)
        {
            Debug.LogWarning(string.Format("{0}.{1} Error: {2}", ProcessingScriptableObjectPack.ModName, ProcessingScriptableObjectPack.obj.name, error_info));
        }

        //static IEnumerator GetDataRequest(string ModName, string file)
        //{
        //    var audio_name = Path.GetFileNameWithoutExtension(file);
        //    var request = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(file, AudioType.WAV);

        //    yield return request.SendWebRequest();

        //    if (request.isNetworkError)
        //        Debug.LogWarningFormat("Load Resource Custom Audio Error {0}", request.error);
        //    else
        //    {
        //        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
        //        clip.name = audio_name;
        //        if (!AudioClipDict.ContainsKey(audio_name))
        //            AudioClipDict.Add(audio_name, clip);
        //        else
        //            UnityEngine.Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}", ModName, audio_name);
        //    }
        //}

        public static AudioClip GetAudioClipFromWav(byte[] raw_data, string clip_name)
        {
            AudioClip clip = null;
            //var raw_data = System.IO.File.ReadAllBytes(file);
            var raw_string = Encoding.ASCII.GetString(raw_data);
            //var clip_name = Path.GetFileNameWithoutExtension(file);

            if (raw_string.Substring(0, 4) == "RIFF")
            {
                if (raw_string.Substring(8, 4) == "WAVE")
                {
                    int index = 4; //ChunkId
                    var ChunkSize = BitConverter.ToUInt32(raw_data, 4);
                    index += 4;
                    index += 4; // WAVE

                    UInt16 NumChannels = 0;
                    UInt32 SampleRate = 0;
                    UInt16 BitsPerSample = 0;
                    UInt16 BolckAlign = 0;

                    while (index < raw_data.Length)
                    {
                        var SubchunkID = raw_string.Substring(index, 4);
                        var SubchunkSize = BitConverter.ToUInt32(raw_data, index + 4);

                        index += 8;
                        if (SubchunkID == "fmt ")
                        {
                            var AudioFormat = BitConverter.ToUInt16(raw_data, index);
                            NumChannels = BitConverter.ToUInt16(raw_data, index + 2);
                            SampleRate = BitConverter.ToUInt32(raw_data, index + 4);
                            var ByteRate = BitConverter.ToUInt32(raw_data, index + 8);
                            BolckAlign = BitConverter.ToUInt16(raw_data, index + 12);
                            BitsPerSample = BitConverter.ToUInt16(raw_data, index + 14);
                            index += (int)SubchunkSize;
                        }
                        else if (SubchunkID == "data")
                        {
                            var data_len = (raw_data.Length - index);
                            var data = new float[data_len / BolckAlign];

                            for (int i = 0; i < data.Length; i++)
                            {
                                if (BitsPerSample == 8)
                                    data[i] = BitConverter.ToChar(raw_data, index + BolckAlign * i) / ((float)Char.MaxValue);
                                else if (BitsPerSample == 16)
                                    data[i] = (BitConverter.ToInt16(raw_data, index + BolckAlign * i)) / ((float)Int16.MaxValue);
                                else if (BitsPerSample == 32)
                                    data[i] = BitConverter.ToInt32(raw_data, index + BolckAlign * i) / ((float)Int32.MaxValue);
                            }

                            clip = AudioClip.Create(clip_name, data.Length, 1, (int)SampleRate, false);
                            clip.SetData(data, 0);

                            index += (int)SubchunkSize;
                            break;
                        }
                        else
                        {
                            index += (int)SubchunkSize;
                        }
                    }
                }
            }
            return clip;
        }

        private static void LoadGameResource()
        {
            try
            {
                foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    if(assembly.GetName().Name == "Assembly-CSharp")
                    {
                        GameSrouceAssembly = assembly;
                        break;
                    }

                var subclasses = from type in GameSrouceAssembly.GetTypes()
                                 where type.IsSubclassOf(typeof(ScriptableObject))
                                 select type;
                foreach (var type in subclasses)
                    ScriptableObjectKeyType.Add(type.Name, type);
            }
            catch
            {

            }

            foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(ScriptableObject)))
            {
                if (ele.GetType().Assembly != GameSrouceAssembly)
                    continue;

                try
                {
                    if (ele is UniqueIDScriptable)
                    {
                        if (!AllScriptableObjectDict.ContainsKey((ele as UniqueIDScriptable).UniqueID))
                            AllScriptableObjectDict.Add((ele as UniqueIDScriptable).UniqueID, ele as ScriptableObject);
                        else
                            UnityEngine.Debug.LogWarning("AllScriptableObjectDict Same Key was Add " + (ele as UniqueIDScriptable).name);
                    }
                    else
                    {
                        if (!AllScriptableObjectDict.ContainsKey(ele.name))
                            AllScriptableObjectDict.Add(ele.name, ele as ScriptableObject);
                        else
                            UnityEngine.Debug.LogWarning("AllScriptableObjectDict Same Key was Add " + (ele as UniqueIDScriptable).name);
                    }

                    if (!(ele is UniqueIDScriptable))
                    {

                        if (!AllScriptableObjectWithoutGuidTypeDict.ContainsKey(ele.GetType()))
                        {
                            AllScriptableObjectWithoutGuidTypeDict.Add(ele.GetType(), new Dictionary<string, ScriptableObject>());
                            if (AllScriptableObjectWithoutGuidTypeDict.TryGetValue(ele.GetType(), out var type_dict))
                                type_dict.Add(ele.name, ele as ScriptableObject);
                        }
                        else
                        {
                            if (AllScriptableObjectWithoutGuidTypeDict.TryGetValue(ele.GetType(), out var type_dict))
                                type_dict.Add(ele.name, ele as ScriptableObject);
                        }
                    }

                    if (ele is UniqueIDScriptable)
                    {
                        if (!AllGUIDTypeDict.ContainsKey(ele.GetType()))
                        {
                            AllGUIDTypeDict.Add(ele.GetType(), new Dictionary<string, UniqueIDScriptable>());
                            if (AllGUIDTypeDict.TryGetValue(ele.GetType(), out var type_dict))
                                type_dict.Add(ele.name, ele as UniqueIDScriptable);
                        }
                        else
                        {
                            if (AllGUIDTypeDict.TryGetValue(ele.GetType(), out var type_dict))
                                type_dict.Add(ele.name, ele as UniqueIDScriptable);
                        }

                        if (!AllGUIDDict.ContainsKey((ele as UniqueIDScriptable).UniqueID))
                            AllGUIDDict.Add((ele as UniqueIDScriptable).UniqueID, ele as UniqueIDScriptable);
                        else
                            UnityEngine.Debug.LogWarning("AllGUIDDict Same Key was Add " + (ele as UniqueIDScriptable).UniqueID);
                    }
                    else if (ele is CardTag)
                    {
                        if (!CardTagDict.ContainsKey(ele.name))
                            CardTagDict.Add(ele.name, ele as CardTag);
                        else
                            UnityEngine.Debug.LogWarning("CardTagDict Same Key was Add " + ele.name);
                    }
                    else if (ele is EquipmentTag)
                    {
                        if (!EquipmentTagDict.ContainsKey(ele.name))
                            EquipmentTagDict.Add(ele.name, ele as EquipmentTag);
                        else
                            UnityEngine.Debug.LogWarning("EquipmentTagDict Same Key was Add " + ele.name);
                    }
                    else if (ele is ActionTag)
                    {
                        if (!ActionTagDict.ContainsKey(ele.name))
                            ActionTagDict.Add(ele.name, ele as ActionTag);
                        else
                            UnityEngine.Debug.LogWarning("ActionTagDict Same Key was Add " + ele.name);
                    }
                    else if (ele is CardTabGroup)
                    {
                        if (!CardTabGroupDict.ContainsKey(ele.name))
                            CardTabGroupDict.Add(ele.name, ele as CardTabGroup);
                        else
                            UnityEngine.Debug.LogWarning("CardTabGroupDict Same Key was Add " + ele.name);
                    }
                    else if (ele is WeatherSet)
                    {
                        if (!WeatherSetDict.ContainsKey(ele.name))
                            WeatherSetDict.Add(ele.name, ele as WeatherSet);
                        else
                            UnityEngine.Debug.LogWarning("WeatherSetDict Same Key was Add " + ele.name);
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning("LoadGameResource Error " + ex.Message);
                }
            }

            foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Sprite)))
            {
                if (!SpriteDict.ContainsKey(ele.name))
                    SpriteDict.Add(ele.name, ele as UnityEngine.Sprite);
                else
                    UnityEngine.Debug.Log("SpriteDict Same Key was Add " + ele.name);
            }
            foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(UnityEngine.AudioClip)))
            {
                if (!AudioClipDict.ContainsKey(ele.name))
                    AudioClipDict.Add(ele.name, ele as UnityEngine.AudioClip);
                else
                    UnityEngine.Debug.Log("AudioClipDict Same Key was Add " + ele.name);
            }
            foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(WeatherSpecialEffect)))
            {
                if (!WeatherSpecialEffectDict.ContainsKey(ele.name))
                    WeatherSpecialEffectDict.Add(ele.name, ele as WeatherSpecialEffect);
                else
                    UnityEngine.Debug.Log("WeatherSpecialEffectDict Same Key was Add " + ele.name);
            }
        }

        private static void LoadModsFromZip()
        {
            try
            {
                var files = Directory.GetFiles(Path.Combine(BepInEx.Paths.BepInExRootPath, "plugins"));
                foreach (var file in files)
                {
                    if (!file.EndsWith(".zip"))
                        continue;
                    ModInfo Info = new ModInfo();
                    string ModName = Path.GetFileNameWithoutExtension(file);
                    string ModDirName = Path.GetFileNameWithoutExtension(file);
                    ZipFile zip = null;
                    //System.Collections.ObjectModel.ReadOnlyCollection<ZipArchiveEntry> entrys = null;
                    ICollection<ZipEntry> entrys = null;

                    //Check if is a Mod Directory and Load Mod Info
                    try
                    {
                        //zip = ZipFile.Open(file, ZipArchiveMode.Read);
                        zip = ZipFile.Read(file);
                        entrys = zip.Entries;
                        ModDirName = entrys.ElementAt(0).FileName.Substring(0, entrys.ElementAt(0).FileName.Length - 1);

                        //var ModInfoZip = zip.GetEntry(ModDirName + @"/ModInfo.json");
                        var ModInfoZip = zip[ModDirName + @"/ModInfo.json"];
                        if (ModInfoZip == null)
                            continue;

                        // Load Mod Info
                        MemoryStream ms = new MemoryStream();
                        ModInfoZip.Extract(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        using (StreamReader sr = new StreamReader(ms))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), Info);

                        if (Info.ModEditorVersion.IsNullOrWhiteSpace())
                            continue;

                        // Check Name
                        if (!Info.Name.IsNullOrWhiteSpace())
                            ModName = Info.Name;

                        UnityEngine.Debug.Log(string.Format("ModLoader Load EditorZipMod {0} {1}", ModName, Info.Version));

                        // Check Verison
                        System.Version ModRequestVersion = System.Version.Parse(Info.ModLoaderVerison);
                        if (PluginVersion.CompareTo(ModRequestVersion) < 0)
                            UnityEngine.Debug.LogWarningFormat("ModLoader Version {0} is lower than {1} Request Version {2}", PluginVersion, ModName, ModRequestVersion);

                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarning("LoadModsFromZip " + ex.Message);
                        continue;
                    }

                    // Load Resource
                    try
                    {
                        foreach (var entry in entrys)
                        {
                            if (!(entry.FileName.StartsWith(ModDirName + @"/Resource") && entry.FileName.EndsWith(".ab")))
                                continue;
                            MemoryStream ms = new MemoryStream();
                            entry.Extract(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            AssetBundle ab = AssetBundle.LoadFromStream(ms);
                            foreach (var obj in ab.LoadAllAssets())
                            {
                                if (obj.GetType() == typeof(UnityEngine.Sprite))
                                {
                                    if (!SpriteDict.ContainsKey(obj.name))
                                        SpriteDict.Add(obj.name, obj as UnityEngine.Sprite);
                                    else
                                        UnityEngine.Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}", ModName, obj.name);
                                }
                                if (obj.GetType() == typeof(UnityEngine.AudioClip))
                                {
                                    if (!AudioClipDict.ContainsKey(obj.name))
                                        AudioClipDict.Add(obj.name, obj as UnityEngine.AudioClip);
                                    else
                                        UnityEngine.Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}", ModName, obj.name);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load Resource Error {1}", ModName, ex.Message);
                    }


                    // Load Resource Custom Pictures
                    try
                    {
                        foreach (var entry in entrys)
                        {
                            if (!(entry.FileName.StartsWith(ModDirName + @"/Resource/Picture") &&
                                (entry.FileName.EndsWith(".jpg") || entry.FileName.EndsWith(".jpeg") || entry.FileName.EndsWith(".png"))))
                                continue;
                            var sprite_name = Path.GetFileNameWithoutExtension(entry.FileName);
                            Texture2D t2d = new Texture2D(2, 2);
                            MemoryStream ms = new MemoryStream();
                            entry.Extract(ms);
                            ImageConversion.LoadImage(t2d, ms.ToArray());
                            Sprite sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
                            sprite.name = sprite_name;
                            if (!SpriteDict.ContainsKey(sprite_name))
                                SpriteDict.Add(sprite_name, sprite);
                            else
                                UnityEngine.Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}", ModName, sprite_name);
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load Resource Custom Pictures Error {1}", ModName, ex.Message);
                    }

                    // Load Resource Custom Audio
                    try
                    {
                        foreach (var entry in entrys)
                        {
                            if (!(entry.FileName.StartsWith(ModDirName + @"/Resource/Audio") && entry.FileName.EndsWith(".wav")))
                                continue;
                            MemoryStream ms = new MemoryStream();
                            entry.Extract(ms);
                            var clip_name = Path.GetFileNameWithoutExtension(entry.FileName);
                            var clip = GetAudioClipFromWav(ms.ToArray(), clip_name);
                            if (clip)
                            {
                                if (!AudioClipDict.ContainsKey(clip.name))
                                    AudioClipDict.Add(clip.name, clip);
                                else
                                    UnityEngine.Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}", ModName, clip.name);
                            }
                            //MBSingleton<GameLoad>.Instance.StartCoroutine(GetDataRequest(ModName, file));

                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load Resource Custom Audio Error {1}", ModName, ex.Message);
                    }

                    // Load ScriptableObject
                    try
                    {
                        var subclasses = from type in GameSrouceAssembly.GetTypes()
                                         where type.IsSubclassOf(typeof(ScriptableObject))
                                         select type;

                        foreach (var type in subclasses)
                        {
                            if (type.IsSubclassOf(typeof(UniqueIDScriptable)) || type == typeof(UniqueIDScriptable))
                                continue;
                            foreach (var entry in entrys)
                            {
                                if (!(entry.FileName.StartsWith(ModDirName + @"/ScriptableObject/" + type.Name) && entry.FileName.EndsWith(".json")))
                                    continue;
                                var obj_name = Path.GetFileNameWithoutExtension(entry.FileName);
                                if (AllScriptableObjectWithoutGuidTypeDict.TryGetValue(type, out var dict))
                                {
                                    if (dict.ContainsKey(obj_name))
                                        continue;
                                    string CardData = "";
                                    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                                    var obj = type.GetMethod("CreateInstance", bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(Type) }, null).Invoke(null, new object[] { type });
                                    MemoryStream ms = new MemoryStream();
                                    entry.Extract(ms);
                                    ms.Seek(0, SeekOrigin.Begin);
                                    using (StreamReader sr = new StreamReader(ms))
                                        CardData = sr.ReadToEnd();
                                    type.GetProperty("name", bindingFlags).GetSetMethod(true).Invoke(obj, new object[] { obj_name });
                                    JsonUtility.FromJsonOverwrite(CardData, obj);
                                    dict.Add(obj_name, obj as ScriptableObject);
                                    WaitForWarpperEditorNoGuidlist.Add(new ScriptableObjectPack(obj as ScriptableObject, "", "", ModName, CardData));
                                    if (!AllScriptableObjectDict.ContainsKey(obj_name))
                                        AllScriptableObjectDict.Add(obj_name, obj as ScriptableObject);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load ScriptableObject Error {1}", ModName, ex.Message);
                    }

                    // Load Localization
                    try
                    {
                        foreach (var entry in entrys)
                        {
                            if (!(entry.FileName.StartsWith(ModDirName + @"/Localization") && entry.FileName.EndsWith(".csv")))
                                continue;
                            MemoryStream ms = new MemoryStream();
                            entry.Extract(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            using (StreamReader sr = new StreamReader(ms))
                                WaitForLoadCSVList.Add(new Tuple<string, string>(Path.GetFileName(entry.FileName), sr.ReadToEnd()));
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load Localization Error {1}", ModName, ex.Message);
                    }

                    // Load and init UniqueIDScriptable
                    try
                    {
                        var subclasses = from type in GameSrouceAssembly.GetTypes()
                                         where type.IsSubclassOf(typeof(UniqueIDScriptable))
                                         select type;

                        foreach (var type in subclasses)
                        {
                            foreach (var entry in entrys)
                            {
                                if (!(entry.FileName.StartsWith(ModDirName + @"/" + type.Name) && entry.FileName.EndsWith(".json")))
                                    continue;
                                string CardName = Path.GetFileNameWithoutExtension(entry.FileName);
                                string CardData = "";
                                try
                                {
                                    JsonData json = new JsonData();
                                    MemoryStream ms = new MemoryStream();
                                    entry.Extract(ms);
                                    ms.Seek(0, SeekOrigin.Begin);
                                    using (StreamReader sr = new StreamReader(ms))
                                    {
                                        CardData = sr.ReadToEnd();
                                        json = JsonMapper.ToObject(CardData);
                                    }
                                    if (!(json.ContainsKey("UniqueID") && json["UniqueID"].IsString && !json["UniqueID"].ToString().IsNullOrWhiteSpace()))
                                    {
                                        UnityEngine.Debug.LogErrorFormat("{0} EditorLoadZip {1} {2} try to load a UniqueIDScriptable without GUID", type.Name, ModName, CardName);
                                        continue;
                                    }

                                    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                                    var card = type.GetMethod("CreateInstance", bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(Type) }, null).Invoke(null, new object[] { type });
                                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                                    JsonUtility.FromJsonOverwrite(CardData, card);

                                    type.GetProperty("name", bindingFlags).GetSetMethod(true).Invoke(card, new object[] { ModName + "_" + CardName });
                                    type.GetMethod("Init", bindingFlags, null, new Type[] { }, null).Invoke(card, null);

                                    var card_guid = type.GetField("UniqueID", bindingFlags).GetValue(card) as string;
                                    AllGUIDDict.Add(card_guid, card as UniqueIDScriptable);
                                    GameLoad.Instance.DataBase.AllData.Add(card as UniqueIDScriptable);

                                    if (!WaitForWarpperEditorGuidDict.ContainsKey(card_guid))
                                        WaitForWarpperEditorGuidDict.Add(card_guid, new ScriptableObjectPack(card as UniqueIDScriptable, "", "", ModName, CardData));
                                    else
                                        UnityEngine.Debug.LogWarningFormat("{0} WaitForWarpperEditorGuidDict Same Key was Add {1}", ModName, card_guid);
                                    if (!AllScriptableObjectDict.ContainsKey(card_guid))
                                        AllScriptableObjectDict.Add(card_guid, card as ScriptableObject);
                                    if (AllGUIDTypeDict.TryGetValue(type, out var dict))
                                        if (!dict.ContainsKey(card_guid))
                                            dict.Add(card_guid, card as UniqueIDScriptable);
                                }
                                catch (Exception ex)
                                {
                                    UnityEngine.Debug.LogWarningFormat("{0} EditorLoadZip {1} {2} Error {3}", type.Name, ModName, CardName, ex.Message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load UniqueIDScriptable Error {1}", ModName, ex.Message);
                    }

                    // Load GameSourceModify
                    try
                    {
                        //var modify_dirs = Directory.GetDirectories(CombinePaths(dir, "GameSourceModify"));
                        foreach (var entry in entrys)
                        {
                            if (!(entry.FileName.StartsWith(ModDirName + @"/GameSourceModify") && entry.FileName.EndsWith(".json")))
                                continue;
                            string CardData = "";
                            string Guid = Path.GetFileNameWithoutExtension(entry.FileName);
                            MemoryStream ms = new MemoryStream();
                            entry.Extract(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            using (StreamReader sr = new StreamReader(ms))
                                CardData = sr.ReadToEnd();
                            if (AllGUIDDict.TryGetValue(Guid, out var obj))
                                WaitForWarpperEditorGameSourceGUIDList.Add(new ScriptableObjectPack(obj, "", "", ModName, CardData));
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load GameSourceModify Error {1}", ModName, ex.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning(ex.Message);
            }
        }

        private static void LoadMods(string mods_dir)
        {
            try
            {
                var dirs = Directory.GetDirectories(mods_dir);
                foreach (var dir in dirs)
                {
                    //  Check if is a Mod Directory
                    if (!File.Exists(CombinePaths(dir, "ModInfo.json")))
                        continue;

                    ModInfo Info = new ModInfo();
                    string ModName = Path.GetFileName(dir);

                    try
                    {
                        // Load Mod Info
                        using (StreamReader sr = new StreamReader(CombinePaths(dir, "ModInfo.json")))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), Info);

                        // Check Name
                        if (!Info.Name.IsNullOrWhiteSpace())
                            ModName = Info.Name;

                        UnityEngine.Debug.Log(string.Format("ModLoader Load Mod {0} {1}", ModName, Info.Version));

                        // Check Verison
                        System.Version ModRequestVersion = System.Version.Parse(Info.ModLoaderVerison);
                        if (PluginVersion.CompareTo(ModRequestVersion) < 0)
                            UnityEngine.Debug.LogWarningFormat("ModLoader Version {0} is lower than {1} Request Version {2}", PluginVersion, ModName, ModRequestVersion);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Check Version Error {1}", ModName, ex.Message);
                    }

                    // Load Resource
                    try
                    {
                        if (Directory.Exists(CombinePaths(dir, "Resource")))
                        {
                            var files = Directory.GetFiles(CombinePaths(dir, "Resource"));
                            foreach (var file in files)
                            {
                                if (!file.EndsWith(".ab"))
                                    continue;
                                AssetBundle ab = AssetBundle.LoadFromFile(file);
                                foreach (var obj in ab.LoadAllAssets())
                                {
                                    if (obj.GetType() == typeof(UnityEngine.Sprite))
                                    {
                                        if (!SpriteDict.ContainsKey(obj.name))
                                            SpriteDict.Add(obj.name, obj as UnityEngine.Sprite);
                                        else
                                            UnityEngine.Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}", ModName, obj.name);
                                    }
                                    if (obj.GetType() == typeof(UnityEngine.AudioClip))
                                    {
                                        if (!AudioClipDict.ContainsKey(obj.name))
                                            AudioClipDict.Add(obj.name, obj as UnityEngine.AudioClip);
                                        else
                                            UnityEngine.Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}", ModName, obj.name);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load Resource Error {1}", ModName, ex.Message);
                    }

                    // Load Resource Custom Pictures
                    try
                    {
                        if (Directory.Exists(CombinePaths(dir, "Resource", "Picture")))
                        {
                            var files = Directory.GetFiles(CombinePaths(dir, "Resource", "Picture"));
                            foreach (var file in files)
                            {
                                if (!file.EndsWith(".jpg") && !file.EndsWith(".jpeg") && !file.EndsWith(".png"))
                                    continue;
                                var sprite_name = Path.GetFileNameWithoutExtension(file);
                                Texture2D t2d = new Texture2D(2, 2);
                                ImageConversion.LoadImage(t2d, System.IO.File.ReadAllBytes(file));
                                Sprite sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
                                sprite.name = sprite_name;
                                if (!SpriteDict.ContainsKey(sprite_name))
                                    SpriteDict.Add(sprite_name, sprite);
                                else
                                    UnityEngine.Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}", ModName, sprite_name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load Resource Custom Pictures Error {1}", ModName, ex.Message);
                    }

                    // Load Resource Custom Audio
                    try
                    {
                        if (Directory.Exists(CombinePaths(dir, "Resource", "Audio")))
                        {
                            var files = Directory.GetFiles(CombinePaths(dir, "Resource", "Audio"));
                            foreach (var file in files)
                            {
                                if (!file.EndsWith(".wav"))
                                    continue;
                                var raw_data = System.IO.File.ReadAllBytes(file);
                                var clip_name = Path.GetFileNameWithoutExtension(file);
                                var clip = GetAudioClipFromWav(raw_data, clip_name);
                                if (clip)
                                {
                                    if (!AudioClipDict.ContainsKey(clip.name))
                                        AudioClipDict.Add(clip.name, clip);
                                    else
                                        UnityEngine.Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}", ModName, clip.name);
                                }
                                //MBSingleton<GameLoad>.Instance.StartCoroutine(GetDataRequest(ModName, file));

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load Resource Custom Audio Error {1}", ModName, ex.Message);
                    }

                    // Load ScriptableObject
                    try
                    {
                        if (Directory.Exists(CombinePaths(dir, "ScriptableObject")))
                        {
                            var subclasses = from type in GameSrouceAssembly.GetTypes()
                                             where type.IsSubclassOf(typeof(ScriptableObject))
                                             select type;

                            foreach (var type in subclasses)
                            {
                                if (type.IsSubclassOf(typeof(UniqueIDScriptable)) || type == typeof(UniqueIDScriptable))
                                    continue;

                                if (!Directory.Exists(CombinePaths(dir, "ScriptableObject", type.Name)))
                                    continue;

                                foreach (var file in Directory.EnumerateFiles(CombinePaths(dir, "ScriptableObject", type.Name), "*.json", SearchOption.AllDirectories))
                                {
                                    var obj_name = Path.GetFileNameWithoutExtension(file);
                                    if (AllScriptableObjectWithoutGuidTypeDict.TryGetValue(type, out var dict))
                                    {
                                        if (dict.ContainsKey(obj_name))
                                            continue;
                                        string CardData = "";
                                        var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                                        var obj = type.GetMethod("CreateInstance", bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(Type) }, null).Invoke(null, new object[] { type });
                                        using (StreamReader sr = new StreamReader(file))
                                            CardData = sr.ReadToEnd();
                                        type.GetProperty("name", bindingFlags).GetSetMethod(true).Invoke(obj, new object[] { obj_name });
                                        JsonUtility.FromJsonOverwrite(CardData, obj);
                                        dict.Add(obj_name, obj as ScriptableObject);
                                        WaitForWarpperEditorNoGuidlist.Add(new ScriptableObjectPack(obj as ScriptableObject, "", "", ModName, CardData));
                                        if (!AllScriptableObjectDict.ContainsKey(obj_name))
                                            AllScriptableObjectDict.Add(obj_name, obj as ScriptableObject);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load ScriptableObject Error {1}", ModName, ex.Message);
                    }

                    // Load Localization
                    try
                    {
                        if (Directory.Exists(CombinePaths(dir, "Localization")))
                        {
                            var files = Directory.GetFiles(CombinePaths(dir, "Localization"));
                            foreach (var file in files)
                            {
                                if (!file.EndsWith(".csv"))
                                    continue;
                                using (StreamReader sr = new StreamReader(file))
                                    WaitForLoadCSVList.Add(new Tuple<string, string>(Path.GetFileName(file), sr.ReadToEnd()));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load Localization Error {1}", ModName, ex.Message);
                    }

                    // Load and init UniqueIDScriptable
                    try
                    {
                        var subclasses = from type in GameSrouceAssembly.GetTypes()
                                         where type.IsSubclassOf(typeof(UniqueIDScriptable))
                                         select type;

                        foreach (var type in subclasses)
                        {
                            if (!Directory.Exists(CombinePaths(dir, type.Name)))
                                continue;
                            if (Info.ModEditorVersion.IsNullOrWhiteSpace())
                            {
                                var card_dirs = Directory.GetDirectories(CombinePaths(dir, type.Name));
                                foreach (var card_dir in card_dirs)
                                {
                                    string CardName = Path.GetFileName(card_dir); ;
                                    string CardPath = CombinePaths(card_dir, CardName + ".json");
                                    string CardData = "";
                                    if (File.Exists(CardPath))
                                    {
                                        try
                                        {
                                            JsonData json = new JsonData();
                                            using (StreamReader sr = new StreamReader(CardPath))
                                            {
                                                CardData = sr.ReadToEnd();
                                                json = JsonMapper.ToObject(CardData);
                                            }
                                            if (!(json.ContainsKey("UniqueID") && json["UniqueID"].IsString && !json["UniqueID"].ToString().IsNullOrWhiteSpace()))
                                            {
                                                UnityEngine.Debug.LogErrorFormat("{0} EditorLoadZip {1} {2} try to load a UniqueIDScriptable without GUID", type.Name, ModName, CardName);
                                                continue;
                                            }

                                            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                                            var card = type.GetMethod("CreateInstance", bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(Type) }, null).Invoke(null, new object[] { type });
                                            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                                            JsonUtility.FromJsonOverwrite(CardData, card);

                                            type.GetProperty("name", bindingFlags).GetSetMethod(true).Invoke(card, new object[] { ModName + "_" + CardName });
                                            type.GetMethod("Init", bindingFlags, null, new Type[] { }, null).Invoke(card, null);

                                            var card_guid = type.GetField("UniqueID", bindingFlags).GetValue(card) as string;
                                            AllGUIDDict.Add(card_guid, card as UniqueIDScriptable);
                                            GameLoad.Instance.DataBase.AllData.Add(card as UniqueIDScriptable);

                                            if (!WaitForWarpperGuidDict.ContainsKey(card_guid))
                                                WaitForWarpperGuidDict.Add(card_guid, new ScriptableObjectPack(card as UniqueIDScriptable, card_dir, CardPath, ModName));
                                            else
                                                UnityEngine.Debug.LogWarningFormat("{0} WaitForWarpperGuidDict Same Key was Add {1}", ModName, card_guid);
                                            if (!AllScriptableObjectDict.ContainsKey(card_guid))
                                                AllScriptableObjectDict.Add(card_guid, card as ScriptableObject);
                                            if (AllGUIDTypeDict.TryGetValue(type, out var dict))
                                                if (!dict.ContainsKey(card_guid))
                                                    dict.Add(card_guid, card as UniqueIDScriptable);
                                        }
                                        catch (Exception ex)
                                        {
                                            UnityEngine.Debug.LogWarningFormat("{0} Load {1} {2} Error {3}", type.Name, ModName, CardName, ex.Message);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var file in Directory.EnumerateFiles(CombinePaths(dir, type.Name), "*.json", SearchOption.AllDirectories))
                                {
                                    string CardName = Path.GetFileNameWithoutExtension(file);
                                    string CardPath = file;
                                    string CardData = "";
                                    try
                                    {
                                        JsonData json = new JsonData();
                                        using (StreamReader sr = new StreamReader(CardPath))
                                        {
                                            CardData = sr.ReadToEnd();
                                            json = JsonMapper.ToObject(CardData);
                                        }
                                        if (!(json.ContainsKey("UniqueID") && json["UniqueID"].IsString && !json["UniqueID"].ToString().IsNullOrWhiteSpace()))
                                        {
                                            UnityEngine.Debug.LogErrorFormat("{0} EditorLoadZip {1} {2} try to load a UniqueIDScriptable without GUID", type.Name, ModName, CardName);
                                            continue;
                                        }

                                        var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                                        var card = type.GetMethod("CreateInstance", bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(Type) }, null).Invoke(null, new object[] { type });
                                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                                        JsonUtility.FromJsonOverwrite(CardData, card);

                                        type.GetProperty("name", bindingFlags).GetSetMethod(true).Invoke(card, new object[] { ModName + "_" + CardName });
                                        type.GetMethod("Init", bindingFlags, null, new Type[] { }, null).Invoke(card, null);

                                        var card_guid = type.GetField("UniqueID", bindingFlags).GetValue(card) as string;
                                        AllGUIDDict.Add(card_guid, card as UniqueIDScriptable);
                                        GameLoad.Instance.DataBase.AllData.Add(card as UniqueIDScriptable);

                                        if (!WaitForWarpperEditorGuidDict.ContainsKey(card_guid))
                                            WaitForWarpperEditorGuidDict.Add(card_guid, new ScriptableObjectPack(card as UniqueIDScriptable, "", CardPath, ModName, CardData));
                                        else
                                            UnityEngine.Debug.LogWarningFormat("{0} WaitForWarpperEditorGuidDict Same Key was Add {1}", ModName, card_guid);
                                        if (!AllScriptableObjectDict.ContainsKey(card_guid))
                                            AllScriptableObjectDict.Add(card_guid, card as ScriptableObject);
                                        if (AllGUIDTypeDict.TryGetValue(type, out var dict))
                                            if (!dict.ContainsKey(card_guid))
                                                dict.Add(card_guid, card as UniqueIDScriptable);
                                    }
                                    catch (Exception ex)
                                    {
                                        UnityEngine.Debug.LogWarningFormat("{0} EditorLoad {1} {2} Error {3}", type.Name, ModName, CardName, ex.Message);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load UniqueIDScriptable Error {1}", ModName, ex.Message);
                    }

                    // Load GameSourceModify
                    try
                    {
                        if (Info.ModEditorVersion.IsNullOrWhiteSpace())
                        {
                            if (Directory.Exists(CombinePaths(dir, "GameSourceModify")))
                            {
                                var modify_dirs = Directory.GetDirectories(CombinePaths(dir, "GameSourceModify"));
                                foreach (var modify_dir in modify_dirs)
                                {
                                    var modify_files = Directory.GetFiles(modify_dir);
                                    if (modify_files.Length == 1 && modify_files[0].EndsWith(".json"))
                                    {
                                        string Guid = Path.GetFileNameWithoutExtension(modify_files[0]);
                                        if (AllGUIDDict.TryGetValue(Guid, out var obj))
                                            WaitForWarpperGameSourceGUIDList.Add(new ScriptableObjectPack(obj, modify_dir, modify_files[0], ModName));
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Directory.Exists(CombinePaths(dir, "GameSourceModify")))
                            {
                                var modify_dirs = Directory.GetDirectories(CombinePaths(dir, "GameSourceModify"));
                                foreach (var modify_dir in modify_dirs)
                                {
                                    var modify_files = Directory.GetFiles(modify_dir);
                                    foreach (var file in modify_files)
                                    {
                                        if (!file.EndsWith(".json"))
                                            continue;
                                        string CardPath = file;
                                        string CardData = "";
                                        string Guid = Path.GetFileNameWithoutExtension(file);
                                        using (StreamReader sr = new StreamReader(CardPath))
                                            CardData = sr.ReadToEnd();
                                        if (AllGUIDDict.TryGetValue(Guid, out var obj))
                                            WaitForWarpperEditorGameSourceGUIDList.Add(new ScriptableObjectPack(obj, modify_dir, CardPath, ModName, CardData));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarningFormat("{0} Load GameSourceModify Error {1}", ModName, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning(ex.Message);
            }
        }

        private static void LoadEditorScriptableObject()
        {
            foreach (var item in WaitForWarpperEditorNoGuidlist)
            {
                try
                {
                    ProcessingScriptableObjectPack = item;
                    JsonData json = new JsonData();
                    json = JsonMapper.ToObject(item.CardData);
                    WarpperFunction.JsonCommonWarpper(item.obj, json);
                    if (item.obj is CardTabGroup && item.obj.name.StartsWith("Tab_"))
                    {
                        WaitForAddCardTabGroup.Add(new ScriptableObjectPack(item.obj, "", "", "", item.CardData));
                    }
                    if(item.obj is ContentPage)
                    {
                        if(item.obj.name.EndsWith("Default"))
                            WaitForAddDefaultContentPage.Add(new ScriptableObjectPack(item.obj, "", "", "", item.CardData));
                        else if(item.obj.name.EndsWith("Main"))
                            WaitForAddMainContentPage.Add(new ScriptableObjectPack(item.obj, "", "", "", item.CardData));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("LoadEditorScriptableObject " + ex.Message);
                }
            }
        }

        private static void WarpperAllMods()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var item in WaitForWarpperGuidDict)
            {
                try
                {
                    ProcessingScriptableObjectPack = item.Value;

                    if (item.Value.obj is CardData)
                    {
                        CardDataWarpper warpper = new CardDataWarpper(item.Value.CardDir);
                        using (StreamReader sr = new StreamReader(item.Value.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.Value.obj as CardData);
                        if ((item.Value.obj as CardData).CardType == CardTypes.Blueprint && !warpper.BlueprintCardDataCardTabGroup.IsNullOrWhiteSpace() && !warpper.BlueprintCardDataCardTabSubGroup.IsNullOrWhiteSpace())
                            WaitForAddBlueprintCard.Add(new Tuple<string, string, CardData>(warpper.BlueprintCardDataCardTabGroup, warpper.BlueprintCardDataCardTabSubGroup, item.Value.obj as CardData));
                        var FillDropsList = typeof(CardData).GetMethod("FillDropsList", bindingFlags);
                        if (FillDropsList != null)
                        {
                            FillDropsList.Invoke(item.Value.obj, null);
                        }
                    }
                    else if (item.Value.obj is CharacterPerk)
                    {
                        CharacterPerkWarpper warpper = new CharacterPerkWarpper(item.Value.CardDir);
                        using (StreamReader sr = new StreamReader(item.Value.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.Value.obj as CharacterPerk);
                        if (!warpper.CharacterPerkPerkGroup.IsNullOrWhiteSpace())
                            WaitForAddPerkGroup.Add(new Tuple<string, CharacterPerk>(warpper.CharacterPerkPerkGroup, item.Value.obj as CharacterPerk));
                    }
                    else if (item.Value.obj is GameStat)
                    {
                        GameStatWarpper warpper = new GameStatWarpper(item.Value.CardDir);
                        using (StreamReader sr = new StreamReader(item.Value.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.Value.obj as GameStat);
                        if (!warpper.VisibleGameStatStatListTab.IsNullOrWhiteSpace())
                            WaitForAddVisibleGameStat.Add(new Tuple<string, GameStat>(warpper.VisibleGameStatStatListTab, item.Value.obj as GameStat));

                    }
                    else if (item.Value.obj is Objective)
                    {
                        ObjectiveWarpper warpper = new ObjectiveWarpper(item.Value.CardDir);
                        using (StreamReader sr = new StreamReader(item.Value.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.Value.obj as Objective);
                    }
                    else if (item.Value.obj is SelfTriggeredAction)
                    {
                        SelfTriggeredActionWarpper warpper = new SelfTriggeredActionWarpper(item.Value.CardDir);
                        using (StreamReader sr = new StreamReader(item.Value.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.Value.obj as SelfTriggeredAction);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("WarpperAllMods " + ex.Message);
                }
            }
        }

        private static void WarpperAllEditorMods()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var item in WaitForWarpperEditorGuidDict)
            {
                try
                {
                    ProcessingScriptableObjectPack = item.Value;

                    JsonData json = new JsonData();
                    json = JsonMapper.ToObject(item.Value.CardData);
                    WarpperFunction.JsonCommonWarpper(item.Value.obj, json);
                    if (item.Value.obj is CardData)
                    {
                        if ((item.Value.obj as CardData).CardType == CardTypes.Blueprint &&
                            json.ContainsKey("BlueprintCardDataCardTabGroup") && json["BlueprintCardDataCardTabGroup"].IsString && !json["BlueprintCardDataCardTabGroup"].ToString().IsNullOrWhiteSpace() &&
                            json.ContainsKey("BlueprintCardDataCardTabSubGroup") && json["BlueprintCardDataCardTabSubGroup"].IsString && !json["BlueprintCardDataCardTabSubGroup"].ToString().IsNullOrWhiteSpace())
                            WaitForAddBlueprintCard.Add(new Tuple<string, string, CardData>(json["BlueprintCardDataCardTabGroup"].ToString(), json["BlueprintCardDataCardTabSubGroup"].ToString(), item.Value.obj as CardData));

                        if (json.ContainsKey("ItemCardDataCardTabGpGroup") && json["ItemCardDataCardTabGpGroup"].IsArray && AllScriptableObjectWithoutGuidTypeDict.TryGetValue(typeof(CardTabGroup), out var dict))
                            for (int i = 0; i < json["ItemCardDataCardTabGpGroup"].Count; i++)
                                if (json["ItemCardDataCardTabGpGroup"][i].IsString && dict.TryGetValue(json["ItemCardDataCardTabGpGroup"][i].ToString(), out var tab_group))
                                    (tab_group as CardTabGroup).IncludedCards.Add(item.Value.obj as CardData);

                        var FillDropsList = typeof(CardData).GetMethod("FillDropsList", bindingFlags);
                        if (FillDropsList != null)
                        {
                            FillDropsList.Invoke(item.Value.obj, null);
                        }
                    }
                    else if (item.Value.obj is CharacterPerk)
                    {
                        if (json.ContainsKey("CharacterPerkPerkGroup") && json["CharacterPerkPerkGroup"].IsString && !json["CharacterPerkPerkGroup"].ToString().IsNullOrWhiteSpace())
                            WaitForAddPerkGroup.Add(new Tuple<string, CharacterPerk>(json["CharacterPerkPerkGroup"].ToString(), item.Value.obj as CharacterPerk));
                    }
                    else if (item.Value.obj is GameStat)
                    {
                        if (json.ContainsKey("VisibleGameStatStatListTab") && json["VisibleGameStatStatListTab"].IsString && !json["VisibleGameStatStatListTab"].ToString().IsNullOrWhiteSpace())
                            WaitForAddVisibleGameStat.Add(new Tuple<string, GameStat>(json["VisibleGameStatStatListTab"].ToString(), item.Value.obj as GameStat));
                    }
                    else if (item.Value.obj is PlayerCharacter)
                    {
                        if (AllGUIDTypeDict.TryGetValue(typeof(Gamemode), out var dict))
                        {
                            foreach (var pair in dict)
                            {
                                var mode = pair.Value as Gamemode;
                                Array.Resize<PlayerCharacter>(ref mode.PlayableCharacters, mode.PlayableCharacters.Length + 1);
                                mode.PlayableCharacters[mode.PlayableCharacters.Length - 1] = item.Value.obj as PlayerCharacter;
                            }
                        }
                        WaitForAddJournalPlayerCharacter.Add(new ScriptableObjectPack(item.Value.obj, "", "", "", item.Value.CardData));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("WarpperAllEditorMods " + ex.Message);
                }
            }
        }

        private static void LoadLocalization()
        {
            if (MBSingleton<LocalizationManager>.Instance.Languages[LocalizationManager.CurrentLanguage].LanguageName == "简体中文")
            {
                foreach (var pair in WaitForLoadCSVList)
                {
                    try
                    {
                        if (pair.Item1.Contains("SimpCn"))
                        {
                            var CurrentTexts = Traverse.Create(MBSingleton<LocalizationManager>.Instance).Field("CurrentTexts").GetValue() as Dictionary<string, string>;
                            Dictionary<string, List<string>> dictionary = CSVParser.LoadFromString(pair.Item2, Delimiter.Comma);
                            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("\\\\n");
                            foreach (KeyValuePair<string, List<string>> keyValuePair in dictionary)
                            {
                                if (!CurrentTexts.ContainsKey(keyValuePair.Key) && keyValuePair.Value.Count >= 2)
                                {
                                    CurrentTexts.Add(keyValuePair.Key, regex.Replace(keyValuePair.Value[1], "\n"));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("LoadLocalization " + ex.Message);
                    }
                }
            }
        }

        private static void AddBlueprintCardData(GraphicsManager instance)
        {
            foreach (var tuple in WaitForAddBlueprintCard)
            {
                try
                {
                    foreach (CardTabGroup group in instance.BlueprintModelsPopup.BlueprintTabs)
                    {
                        if (group.name == tuple.Item1)
                        {
                            group.ShopSortingList.Add(tuple.Item3);
                            foreach (CardTabGroup sub_group in group.SubGroups)
                            {
                                if (sub_group.name == tuple.Item2)
                                {
                                    sub_group.IncludedCards.Add(tuple.Item3);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("AddBlueprintCardData " + ex.Message);
                }
            }
        }

        private static void AddVisibleGameStat(GraphicsManager instance)
        {
            foreach (var tuple in WaitForAddVisibleGameStat)
            {
                try
                {
                    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    var StatList = instance.AllStatsList.GetType().GetField("Tabs", bindingFlags).GetValue(instance.AllStatsList) as StatListTab[];
                    foreach (StatListTab list in StatList)
                    {
                        if (list.name == tuple.Item1)
                        {
                            list.ContainedStats.Add(tuple.Item2);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("AddVisibleGameStat " + ex.Message);
                }
            }
        }

        private static void AddPerkGroup()
        {
            foreach (var tuple in WaitForAddPerkGroup)
            {
                try
                {
                    if (AllGUIDTypeDict.TryGetValue(typeof(PerkGroup), out var dict))
                    {
                        if (dict.TryGetValue(tuple.Item1, out var group))
                        {
                            var obj = group as PerkGroup;
                            Array.Resize<CharacterPerk>(ref obj.PerksList, obj.PerksList.Length + 1);
                            obj.PerksList[obj.PerksList.Length - 1] = tuple.Item2;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("AddPerkGroup " + ex.Message);
                }
            }
        }

        private static void AddCardTabGroup(GraphicsManager instance)
        {
            foreach (var item in WaitForAddCardTabGroup)
            {
                try
                {
                    if (!(item.obj is CardTabGroup))
                        continue;

                    var obj = item.obj as CardTabGroup;

                    obj.FillSortingList();

                    if (!obj.name.StartsWith("Tab_"))
                        continue;

                    if (obj.SubGroups.Count != 0)
                    {
                        Array.Resize<CardTabGroup>(ref instance.BlueprintModelsPopup.BlueprintTabs, instance.BlueprintModelsPopup.BlueprintTabs.Length + 1);
                        instance.BlueprintModelsPopup.BlueprintTabs[instance.BlueprintModelsPopup.BlueprintTabs.Length - 1] = obj;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("AddCardTabGroup " + ex.Message);
                }
            }
        }

        private static void AddCardTabGroupOnce(GraphicsManager instance)
        {
            foreach (var item in WaitForAddCardTabGroup)
            {
                try
                {
                    if (!(item.obj is CardTabGroup))
                        continue;

                    var obj = item.obj as CardTabGroup;

                    obj.FillSortingList();

                    if (!obj.name.StartsWith("Tab_"))
                        continue;

                    if (obj.SubGroups.Count == 0)
                    {
                        JsonData json = new JsonData();
                        json = JsonMapper.ToObject(item.CardData);
                        if (json.ContainsKey("BlueprintCardDataCardTabGroup") && json["BlueprintCardDataCardTabGroup"].IsString && !json["BlueprintCardDataCardTabGroup"].ToString().IsNullOrWhiteSpace())
                        {
                            foreach (CardTabGroup group in instance.BlueprintModelsPopup.BlueprintTabs)
                            {
                                if (group.name == json["BlueprintCardDataCardTabGroup"].ToString())
                                {
                                    group.SubGroups.Add(obj);
                                    group.FillSortingList();
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("AddCustomCardTabGroup " + ex.Message);
                }
            }
        }

        private static void WarpperAllGameSrouces()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var item in WaitForWarpperGameSourceGUIDList)
            {
                try
                {
                    ProcessingScriptableObjectPack = item;

                    if (item.obj is CardData)
                    {
                        CardDataWarpper warpper = new CardDataWarpper(item.CardDir);
                        using (StreamReader sr = new StreamReader(item.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.obj as CardData);
                        var FillDropsList = typeof(CardData).GetMethod("FillDropsList", bindingFlags);
                        if (FillDropsList != null)
                        {
                            FillDropsList.Invoke(item.obj, null);
                        }
                    }
                    else if (item.obj is CharacterPerk)
                    {
                        CharacterPerkWarpper warpper = new CharacterPerkWarpper(item.CardDir);
                        using (StreamReader sr = new StreamReader(item.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.obj as CharacterPerk);
                    }
                    else if (item.obj is GameStat)
                    {
                        GameStatWarpper warpper = new GameStatWarpper(item.CardDir);
                        using (StreamReader sr = new StreamReader(item.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.obj as GameStat);

                    }
                    else if (item.obj is Objective)
                    {
                        ObjectiveWarpper warpper = new ObjectiveWarpper(item.CardDir);
                        using (StreamReader sr = new StreamReader(item.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.obj as Objective);
                    }
                    else if (item.obj is SelfTriggeredAction)
                    {
                        SelfTriggeredActionWarpper warpper = new SelfTriggeredActionWarpper(item.CardDir);
                        using (StreamReader sr = new StreamReader(item.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.obj as SelfTriggeredAction);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("WarpperAllGameSrouces " + ex.Message);
                }
            }
        }

        private static void WarpperAllEditorGameSrouces()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var item in WaitForWarpperEditorGameSourceGUIDList)
            {
                try
                {
                    ProcessingScriptableObjectPack = item;

                    JsonData json = new JsonData();
                    if (!item.CardData.IsNullOrWhiteSpace())
                    {
                        json = JsonMapper.ToObject(item.CardData);
                        if (json.ContainsKey("MatchTagWarpData") && json["MatchTagWarpData"].IsArray && json["MatchTagWarpData"].Count > 0)
                        {
                            WaitForMatchAndWarpperEditorGameSourceList.Add(item);
                            continue;
                        }
                        if (json.ContainsKey("ModLoaderSpecialOverwrite"))
                            if (json["ModLoaderSpecialOverwrite"].IsBoolean && (bool)json["ModLoaderSpecialOverwrite"])
                                JsonUtility.FromJsonOverwrite(item.CardData, item.obj);
                        WarpperFunction.JsonCommonWarpper(item.obj, json);
                    }

                    if (item.obj is CardData)
                    {
                        var FillDropsList = typeof(CardData).GetMethod("FillDropsList", bindingFlags);
                        if (FillDropsList != null)
                        {
                            FillDropsList.Invoke(item.obj, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("WarpperAllEditorGameSrouces " + ex.Message);
                }
            }
        }

        private static void MatchAndWarpperAllEditorGameSrouce()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var item in AllGUIDDict.Values)
            {
                try
                {
                    if (item is CardData)
                    {
                        var obj = item as CardData;
                        foreach (var tag in obj.CardTags)
                        {
                            if (!AllCardTagGuidCardDataDict.ContainsKey(tag.name))
                                AllCardTagGuidCardDataDict.Add(tag.name, new Dictionary<string, CardData>());

                            if (AllCardTagGuidCardDataDict.TryGetValue(tag.name, out var dict))
                                dict.Add(obj.UniqueID, obj);
                        }
                    }
                }
                catch
                {
                    //Debug.LogWarning("MatchAndWarpperAllEditorGameSrouce Match " + ex.Message);
                }
            }

            foreach (var item in WaitForMatchAndWarpperEditorGameSourceList)
            {
                try
                {
                    JsonData json = new JsonData();
                    if (item.CardData.IsNullOrWhiteSpace())
                        continue;
                    json = JsonMapper.ToObject(item.CardData);

                    if (json.ContainsKey("MatchTagWarpData") && json["MatchTagWarpData"].IsArray && json["MatchTagWarpData"].Count > 0)
                    {
                        if (!AllCardTagGuidCardDataDict.TryGetValue(json["MatchTagWarpData"][0].ToString(), out var dict))
                            continue;
                        var MatchList = dict.Keys.ToList();

                        for (int i = 1; i < json["MatchTagWarpData"].Count; i++)
                        {
                            if (AllCardTagGuidCardDataDict.TryGetValue(json["MatchTagWarpData"][i].ToString(), out var next_dict))
                                MatchList = MatchList.Intersect(next_dict.Keys).ToList();
                        }

                        foreach (var match in MatchList)
                        {
                            if (AllGUIDDict.TryGetValue(match, out var card))
                            {
                                if (card is CardData)
                                {
                                    if (json.ContainsKey("MatchTypeWarpData") && json["MatchTypeWarpData"].IsString)
                                        if ((card as CardData).CardType.ToString() != json["MatchTypeWarpData"].ToString())
                                            continue;
                                    WarpperFunction.JsonCommonWarpper(card, json);
                                    var FillDropsList = typeof(CardData).GetMethod("FillDropsList", bindingFlags);
                                    if (FillDropsList != null)
                                        FillDropsList.Invoke(card, null);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("MatchAndWarpperAllEditorGameSrouce Warpper " + ex.Message);
                }
            }
        }

        static IEnumerator WaiterForContentDisplayer()
        {
            bool done = false;
            while (!done)
            {
                yield return new WaitForSeconds(1.0f);
                var objs = Resources.FindObjectsOfTypeAll(typeof(GameObject));
                foreach (var obj in objs)
                {
                    if (obj.name == "JournalTourist")
                    {
                        GameObject clone = null;
                        ContentDisplayer displayer = null;
                        try
                        {
                            clone = UnityEngine.Object.Instantiate(obj) as GameObject;
                            displayer = clone.GetComponent(typeof(ContentDisplayer)) as ContentDisplayer;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("FXMask Warning " + ex.Message);
                        }
                        if (displayer == null)
                            break;
                        clone.name = "JournalDefaultSample";
                        clone.hideFlags = HideFlags.HideAndDontSave;
                        CustomGameObjectListDict.Add(clone.name, clone);
                        CustomContentDisplayerDict.Add(clone.name, displayer);
                        done = true;
                        break;
                    }
                }
            }

            var displayers = Resources.FindObjectsOfTypeAll(typeof(ContentDisplayer));
            foreach (var displayer in displayers)
            {
                try
                {
                    if (!CustomContentDisplayerDict.ContainsKey(displayer.name))
                        CustomContentDisplayerDict.Add(displayer.name, displayer as ContentDisplayer);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("CustomContentDisplayerDict Warning " + ex.Message);
                }
            }

            foreach (var item in WaitForAddDefaultContentPage)
            {
                try
                {
                    if (CustomGameObjectListDict.ContainsKey(item.obj.name))
                        continue;
                    if (CustomGameObjectListDict.TryGetValue("JournalDefaultSample", out var sample))
                    {
                        GameObject clone = null;
                        ContentDisplayer displayer = null;
                        try
                        {
                            clone = UnityEngine.Object.Instantiate(sample) as GameObject;
                            displayer = clone.GetComponent(typeof(ContentDisplayer)) as ContentDisplayer;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("FXMask Warning " + ex.Message);
                        }
                        var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                        var pages = typeof(ContentDisplayer).GetField("ExplicitPageContent", bindingFlags).GetValue(displayer) as IList;
                        pages.Clear();
                        pages.Add(item.obj);
                        typeof(ContentDisplayer).GetField("DefaultPage", bindingFlags).SetValue(displayer, item.obj);

                        var name_parts = item.obj.name.Split('_');
                        if (name_parts.Length > 2)
                        {
                            clone.name = name_parts[0] + "_" + name_parts[1];
                            clone.hideFlags = HideFlags.HideAndDontSave;
                            CustomGameObjectListDict.Add(clone.name, clone);
                            CustomContentDisplayerDict.Add(clone.name, displayer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("WaiterForContentDisplayer WaitForAddDefaultContentPage " + ex.Message);
                }
            }

            foreach (var item in WaitForAddMainContentPage)
            {
                try
                {
                    var name_parts = item.obj.name.Split('_');
                    if (name_parts.Length > 2)
                    {
                        if (CustomContentDisplayerDict.TryGetValue(name_parts[0] + "_" + name_parts[1], out var displayer))
                        {
                            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                            var pages = typeof(ContentDisplayer).GetField("ExplicitPageContent", bindingFlags).GetValue(displayer) as IList;
                            pages.Add(item.obj);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("WaiterForContentDisplayer WaitForAddMainContentPage " + ex.Message);
                }
            }

            foreach (var item in WaitForAddJournalPlayerCharacter)
            {
                try
                {
                    if (!(item.obj is PlayerCharacter))
                        continue;

                    JsonData json = new JsonData();
                    json = JsonMapper.ToObject(item.CardData);
                    if (json.ContainsKey("PlayerCharacterJournalName") && json["PlayerCharacterJournalName"].IsString && !json["PlayerCharacterJournalName"].ToString().IsNullOrWhiteSpace())
                    {
                        if (CustomContentDisplayerDict.TryGetValue(json["PlayerCharacterJournalName"].ToString(), out var displayer))
                        {
                            (item.obj as PlayerCharacter).Journal = displayer;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("WaiterForContentDisplayer PlayerCharacterJournalName " + ex.Message);
                }
            }
        }

        private static void CustomGameObjectFixed()
        {

            foreach (var item in CustomGameObjectListDict)
            {
                try
                {
                    var transform = item.Value.transform.Find("Shadow/GuideFrame/GuideContentPage/Content/Horizontal");
                    if (transform != null)
                    {
                        for (int i = 0; i < transform.childCount; i++)
                        {
                            GameObject.Destroy(transform.GetChild(i).gameObject);
                        }
                    }
                    transform = item.Value.transform.Find("Shadow/GuideFrame");
                    var fx = transform.gameObject.GetComponent(typeof(FXMask)) as FXMask;
                    fx.enabled = true;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("CustomGameObjectFixed " + ex.Message);
                }
            }
        }

        private static void AddPlayerCharacter(GameLoad instance)
        {
            try
            {
                instance.StartCoroutine(WaiterForContentDisplayer());
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(Gamemode), "GetStartingCards")]
        //public static void GamemodeGetStartingCardsPostfix()
        //{
        //    try
        //    {
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.LogWarning(ex.Message);
        //    }
        //}

        [HarmonyPostfix, HarmonyPatch(typeof(GameLoad), "LoadGameData")]
        public static void GameLoadLoadGameDataPostfix(GameLoad __instance)
        {
            try
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                LoadGameResource();

                LoadMods(Path.Combine(BepInEx.Paths.BepInExRootPath, "plugins"));

                LoadModsFromZip();

                LoadEditorScriptableObject();

                LoadLocalization();

                WarpperAllMods();

                WarpperAllEditorMods();

                WarpperAllGameSrouces();

                WarpperAllEditorGameSrouces();

                MatchAndWarpperAllEditorGameSrouce();

                AddPerkGroup();

                AddPlayerCharacter(__instance);

                stopwatch.Stop();
                Debug.Log("ModLoader Time taken: " + (stopwatch.Elapsed));
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(LocalizationManager), "LoadLanguage")]
        public static void LocalizationManagerLoadLanguagePostfix()
        {
            try
            {
                LoadLocalization();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        }

        // No work
        //[HarmonyPrefix, HarmonyPatch(typeof(BlueprintModelsScreen), "Awake")]
        //public static void BlueprintModelsScreenAwakePrefix(BlueprintModelsScreen __instance)
        //{
        //    AddCardTabGroup(__instance);
        //}

        private static bool init_flag = false;

        [HarmonyPostfix, HarmonyPatch(typeof(GraphicsManager), "Init")]
        public static void GraphicsManagerInitPostfix(GraphicsManager __instance)
        {
            try
            {
                AddCardTabGroup(__instance);

                AddBlueprintCardData(__instance);

                AddVisibleGameStat(__instance);

                if (!init_flag)
                {
                    AddCardTabGroupOnce(__instance);

                    CustomGameObjectFixed();
                    init_flag = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        }
    }
}
