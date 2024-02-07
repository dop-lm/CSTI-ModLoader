using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ChatTreeLoader.Patchers;
using CSTI_LuaActionSupport;
using CSTI_LuaActionSupport.DataStruct;
using HarmonyLib;
using Ionic.Zip;
using LitJson;
using ModLoader.Compatible;
using ModLoader.ExportUtil;
using ModLoader.LoaderUtil;
using ModLoader.UI;
using ModLoader.Updater;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace ModLoader;

[Serializable]
public class ModInfo
{
    public string Name;
    public string Version;

    public string ModLoaderVerison;

    public string ModEditorVersion;
}

public class ModPack
{
    public readonly ModInfo ModInfo;
    public readonly string FileName;
    public readonly ConfigEntry<bool> EnableEntry;
    public bool Loaded;

    public ModPack(ModInfo modInfo, string fileName, ConfigEntry<bool> enableEntry, bool loaded)
    {
        ModInfo = modInfo;
        FileName = fileName;
        EnableEntry = enableEntry;
        Loaded = loaded;
    }
}

[BepInPlugin("Dop.plugin.CSTI.ModLoader", "ModLoader", ModVersion)]
[BepInDependency("zender.LuaActionSupport.LuaSupportRuntime")]
[SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
public class ModLoader : BaseUnityPlugin
{
    public const string ModVersion = "2.3.6.22";

    public static readonly Dictionary<string, Dictionary<string, string>> AllLuaFiles = new();

    static ModLoader()
    {
        try
        {
            LuaSupportRuntime.Init(SpriteDict, AllLuaFiles);
            NormalPatcher.DoPatch(HarmonyInstance);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public static ManualLogSource CommonLogger { get; private set; } = new("ModLoader");

    public static readonly Dictionary<string, KVProvider> UniqueIdObjectExtraData = new();
    public static readonly Dictionary<int, KVProvider> ScriptableObjectExtraData = new();
    public static readonly Dictionary<object, KVProvider> ClassObjectExtraData = new();

    public static readonly AccessTools.FieldRef<Dictionary<string, string>> CurrentTextsFieldRef =
        AccessTools.StaticFieldRefAccess<Dictionary<string, string>>(AccessTools.Field(typeof(LocalizationManager),
            "CurrentTexts"));

    public static ConfigEntry<bool> TexCompatibilityMode;
    public static readonly Dictionary<string, ModPack> ModPacks = new();

    public static Version PluginVersion;
    public static Assembly GameSourceAssembly;
    public static readonly Harmony HarmonyInstance = new("Dop.plugin.CSTI.ModLoader");
    public static ModLoader ModLoaderInstance;

    public static readonly Dictionary<string, Sprite> SpriteDict = new();

    public static readonly Dictionary<string, AudioClip> AudioClipDict = new();

    public static readonly Dictionary<string, WeatherSpecialEffect> WeatherSpecialEffectDict = new();

    // GUID Dict
    public static readonly Dictionary<string, UniqueIDScriptable> AllGUIDDict = new();

    public static readonly Dictionary<Type, Dictionary<string, UniqueIDScriptable>> AllGUIDTypeDict = new();

    public static readonly Dictionary<string, Dictionary<string, CardData>> AllCardTagGuidCardDataDict = new();

    // ScriptableObject Dict
    public static readonly Dictionary<string, ScriptableObject> AllScriptableObjectDict = new();

    public static readonly Dictionary<Type, Dictionary<string, ScriptableObject>>
        AllScriptableObjectWithoutGuidTypeDict = new();

    public static readonly Dictionary<string, Type> ScriptableObjectKeyType = new();

    // Special Dict(Vulnerable Function)
    public static readonly Dictionary<string, ContentDisplayer> CustomContentDisplayerDict = new();

    public static readonly Dictionary<string, GameObject> CustomGameObjectListDict = new();

    public struct ScriptableObjectPack
    {
        public ScriptableObject? obj;
        public readonly string CardDirOrGuid;
        public string CardPath;
        public readonly string ModName;
        public readonly KVProvider? CardData;

        public ScriptableObjectPack(ScriptableObject? obj,
            string CardDirOrGuid,
            string CardPath,
            string ModName,
            KVProvider? CardData)
        {
            this.obj = obj;
            this.CardDirOrGuid = CardDirOrGuid;
            this.CardPath = CardPath;
            this.ModName = ModName;
            this.CardData = CardData;
        }
    }

    public static ScriptableObjectPack ProcessingScriptableObjectPack;

    public static readonly Dictionary<string, ScriptableObjectPack> WaitForWarpperEditorGuidDict = new();

    public static readonly List<ScriptableObjectPack> WaitForWarpperEditorNoGuidList = new();

    public static readonly List<ScriptableObjectPack> WaitForWarpperEditorGameSourceGUIDList = new();

    public static readonly List<ScriptableObjectPack> WaitForMatchAndWarpperEditorGameSourceList = new();

    public static readonly List<(string LocalName, string LocalContent)> WaitForLoadCSVList = new();

    public static readonly List<Tuple<string, string, CardData>> WaitForAddBlueprintCard = new();

    public static readonly List<Tuple<string, CardData>>
        WaitForAddCardFilterGroupCard = new();

    public static readonly List<Tuple<string, GameStat>> WaitForAddVisibleGameStat = new();
    private static readonly List<GuideEntry> WaitForAddGuideEntry = new();

    public static readonly List<Tuple<string, CharacterPerk>>
        WaitForAddPerkGroup = new();

    private static readonly List<ScriptableObjectPack> WaitForAddCardTabGroup = new();
    public static readonly List<ScriptableObjectPack> WaitForAddJournalPlayerCharacter = new();
    private static readonly List<ScriptableObjectPack> WaitForAddDefaultContentPage = new();
    private static readonly List<ScriptableObjectPack> WaitForAddMainContentPage = new();
    public static bool HasEncounterType;
    public static Image MainUIBackPanel;
    public static RectTransform MainUIBackPanelRT;
    public bool ModLoaderUpdated;

    private void Start()
    {
        StartCoroutine(FontLoader());
        
        CompatibleCheck.MainCheck();
    }

    public static AssetBundle FontAssetBundle;

    private static IEnumerator FontLoader()
    {
        AssetBundleCreateRequest assetBundleCreateRequest;
        try
        {
            assetBundleCreateRequest = AssetBundle.LoadFromStreamAsync(EmbeddedResources.CSTIFonts);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            yield break;
        }

        yield return assetBundleCreateRequest;
        FontAssetBundle = assetBundleCreateRequest.assetBundle;
        var assetBundleRequest = FontAssetBundle.LoadAssetAsync<TMP_FontAsset>("SourceHanSerifCN-SemiBold SDF");
        yield return assetBundleRequest;
        var font = assetBundleRequest.asset as TMP_FontAsset;
        font!.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        var toAddFallback = new HashSet<TMP_FontAsset>(Resources.FindObjectsOfTypeAll<FontSet>()
            .SelectMany(set => set.Settings).Select(settings => settings.FontObject));

        foreach (var fontAsset in toAddFallback)
        {
            if (fontAsset.name == "LiberationSans SDF") continue;
            if (fontAsset.fallbackFontAssetTable == null)
                fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset> {font};
            else
                fontAsset.fallbackFontAssetTable.Add(font);
        }
    }

    private void Awake()
    {
        ModLoaderInstance = this;
        try
        {
            StartCoroutine(AutoUpdate.UpdateModIfNecessary());
        }
        catch (Exception e)
        {
            Logger.LogWarning(e);
        }

        MainUI.CreatePanel();
        MainUIBackPanelRT.sizeDelta = new Vector2(1920, 1080) * 0.55f;
        MainUIBackPanelRT.position =
            new Vector2(1920 * 0.12f + 1920 * 0.55f * 0.5f,
                1080 * (1 - 0.12f) - 1080 * 0.55f * 0.5f);
        MainUIBackPanelRT.gameObject.SetActive(false);

        this.StartCoroutineEx(PostSpriteLoad.CompressOnLate(), out var controller);
        PostSpriteLoad.Controller = controller;
        Config.Bind("是否将加载的纹理设置为只读", "SetTexture2ReadOnly", false,
            "将加载的纹理设置为只读可以减少内存使用但是之后不能再读取纹理");
        TexCompatibilityMode = Config.Bind("兼容性设置", "TexCompatibilityMode", false,
            "开启后纹理占用内存会增加，请仅在缺图时开启");
        // Plugin startup logic
        if (AccessTools.TypeByName("EncounterPopup") != null)
        {
            MainPatcher.DoPatch(HarmonyInstance);
            HasEncounterType = true;
        }

        foreach (var type in AccessTools.AllTypes())
        {
            if (!type.IsSubclassOf(typeof(ScriptableObject))) continue;

            var hasSerializable = false;
            foreach (var customAttributeData in type.CustomAttributes)
                if (customAttributeData.AttributeType == typeof(SerializableAttribute))
                    hasSerializable = true;

            if (hasSerializable)
                AllScriptableObjectWithoutGuidTypeDict[type] = new Dictionary<string, ScriptableObject>();
        }

        PluginVersion = Version.Parse(Info.Metadata.Version.ToString());

        try
        {
            // var UniqueIDScriptableClearDictPrefixMethod =
            //     new HarmonyMethod(typeof(ModLoader).GetMethod("UniqueIDScriptableClearDictPrefix"));
            var UniqueIDScriptableClearDictPrefixMethod =
                new HarmonyMethod(typeof(ModLoader), nameof(UniqueIDScriptableClearDictPrefix));
            // harmony.Patch(typeof(UniqueIDScriptable).GetMethod("ClearDict", bindingFlags),
            //     prefix: UniqueIDScriptableClearDictPrefixMethod);
            HarmonyInstance.Patch(AccessTools.Method(typeof(UniqueIDScriptable), "ClearDict"),
                UniqueIDScriptableClearDictPrefixMethod);
        }
        catch (Exception ex)
        {
            Debug.LogWarningFormat("{0} {1}", "UniqueIDScriptableClearDictPrefix", ex);
        }

        try
        {
            var LocalizationManagerLoadLanguagePostfixMethod =
                new HarmonyMethod(typeof(ModLoader), nameof(LocalizationManagerLoadLanguagePostfix));
            // harmony.Patch(typeof(LocalizationManager).GetMethod("LoadLanguage", bindingFlags),
            //     postfix: LocalizationManagerLoadLanguagePostfixMethod);
            HarmonyInstance.Patch(AccessTools.Method(typeof(LocalizationManager), "LoadLanguage"),
                postfix: LocalizationManagerLoadLanguagePostfixMethod);
        }
        catch (Exception ex)
        {
            Debug.LogWarningFormat("{0} {1}", "LocalizationManagerLoadLanguagePostfix", ex);
        }

        try
        {
            var GuideManagerStartPrefixMethod =
                new HarmonyMethod(typeof(ModLoader), nameof(GuideManagerStartPrefix));
            // harmony.Patch(typeof(GuideManager).GetMethod("Start", bindingFlags),
            //     prefix: GuideManagerStartPrefixMethod);
            HarmonyInstance.Patch(AccessTools.Method(typeof(GuideManager), "Start"),
                GuideManagerStartPrefixMethod);
        }
        catch (Exception ex)
        {
            Debug.LogWarningFormat("{0} {1}", "GuideManagerStartPrefix", ex);
        }


        try
        {
            var GraphicsManagerInitPostfixMethod =
                new HarmonyMethod(typeof(ModLoader), nameof(GraphicsManagerInitPostfix));
            // harmony.Patch(typeof(GraphicsManager).GetMethod("Init", bindingFlags),
            //     postfix: GraphicsManagerInitPostfixMethod);
            HarmonyInstance.Patch(AccessTools.Method(typeof(GraphicsManager), "Init"),
                postfix: GraphicsManagerInitPostfixMethod);
        }
        catch (Exception ex)
        {
            Debug.LogWarningFormat("{0} {1}", "GraphicsManagerInitPostfix", ex);
        }

        try
        {
            var fixFXMaskAwake = new HarmonyMethod(typeof(ModLoader), nameof(FixFXMaskAwake));
            HarmonyInstance.Patch(AccessTools.Method(typeof(FXMask), "Awake"), fixFXMaskAwake);
        }
        catch (Exception e)
        {
            Logger.LogWarning(e);
        }


        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            if (assembly.GetName().Name == "Assembly-CSharp")
            {
                GameSourceAssembly = assembly;
                break;
            }

        Logger.LogInfo("Plugin ModLoader is loaded! ");
        LoadPreData.LoadData(Path.Combine(Paths.BepInExRootPath, "plugins"));
        Logger.LogInfo("ModLoader Resource Preload being");
    }

    public static string CombinePaths(params string[] paths)
    {
        if (paths == null) throw new ArgumentNullException("paths");

        return paths.Aggregate(Path.Combine);
    }

    public static bool IsSubDirectory(string dir, string parent_dir)
    {
        var di1 = new DirectoryInfo(parent_dir);
        var di2 = new DirectoryInfo(dir);
        var isParent = false;
        while (di2.Parent != null)
        {
            if (di2.Parent.FullName == di1.FullName)
            {
                isParent = true;
                break;
            }

            di2 = di2.Parent;
        }

        return isParent;
    }

    public static void LogErrorWithModInfo(string error_info)
    {
        Debug.LogWarning(string.Format("{0}.{1} Error: {2}", ProcessingScriptableObjectPack.ModName,
            ProcessingScriptableObjectPack.obj.name, error_info));
    }

    private static void LoadGameResource()
    {
        try
        {
            var subclasses = from type in GameSourceAssembly.GetTypes()
                where type.IsSubclassOf(typeof(ScriptableObject))
                select type;
            foreach (var type in subclasses)
                ScriptableObjectKeyType.Add(type.Name, type);
        }
        catch
        {
            // ignored
        }

        foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(ScriptableObject)))
        {
            if (ele.GetType().Assembly != GameSourceAssembly)
                continue;

            try
            {
                if (ele is UniqueIDScriptable scriptable)
                {
                    if (!AllScriptableObjectDict.ContainsKey(scriptable.UniqueID))
                        AllScriptableObjectDict.Add(scriptable.UniqueID, scriptable);
                    else
                        Debug.LogWarning("AllScriptableObjectDict Same Key was Add " +
                                         scriptable.name);
                }
                else
                {
                    if (!AllScriptableObjectDict.ContainsKey(ele.name))
                        AllScriptableObjectDict.Add(ele.name, ele as ScriptableObject);
                    else
                        Debug.LogWarning("AllScriptableObjectDict Same Key was Add " +
                                         (ele as UniqueIDScriptable).name);
                }

                if (ele is not UniqueIDScriptable)
                {
                    if (!AllScriptableObjectWithoutGuidTypeDict.ContainsKey(ele.GetType()))
                    {
                        AllScriptableObjectWithoutGuidTypeDict.Add(ele.GetType(),
                            new Dictionary<string, ScriptableObject>());
                        if (AllScriptableObjectWithoutGuidTypeDict.TryGetValue(ele.GetType(), out var type_dict))
                            type_dict.Add(ele.name, ele as ScriptableObject);
                    }
                    else
                    {
                        if (AllScriptableObjectWithoutGuidTypeDict.TryGetValue(ele.GetType(), out var type_dict))
                            type_dict.Add(ele.name, ele as ScriptableObject);
                    }
                }

                if (ele is UniqueIDScriptable idScriptable)
                {
                    if (!AllGUIDTypeDict.ContainsKey(idScriptable.GetType()))
                    {
                        AllGUIDTypeDict.Add(idScriptable.GetType(), new Dictionary<string, UniqueIDScriptable>());
                        if (AllGUIDTypeDict.TryGetValue(idScriptable.GetType(), out var type_dict))
                            type_dict.Add(idScriptable.name, idScriptable);
                    }
                    else
                    {
                        if (AllGUIDTypeDict.TryGetValue(idScriptable.GetType(), out var type_dict))
                            type_dict.Add(idScriptable.name, idScriptable);
                    }

                    if (!AllGUIDDict.ContainsKey(idScriptable.UniqueID))
                        AllGUIDDict.Add(idScriptable.UniqueID, idScriptable);
                    else
                        Debug.LogWarning("AllGUIDDict Same Key was Add " +
                                         idScriptable.UniqueID);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("LoadGameResource Error " + ex.Message);
            }
        }

        foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(Sprite)))
            if (!SpriteDict.ContainsKey(ele.name))
                SpriteDict.Add(ele.name, ele as Sprite);
            else
                Debug.Log("SpriteDict Same Key was Add " + ele.name);

        foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(AudioClip)))
            if (!AudioClipDict.ContainsKey(ele.name))
                AudioClipDict.Add(ele.name, ele as AudioClip);
            else
                Debug.Log("AudioClipDict Same Key was Add " + ele.name);

        foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(WeatherSpecialEffect)))
            if (!WeatherSpecialEffectDict.ContainsKey(ele.name))
                WeatherSpecialEffectDict.Add(ele.name, ele as WeatherSpecialEffect);
            else
                Debug.Log("WeatherSpecialEffectDict Same Key was Add " + ele.name);
    }

    private static void LoadModsFromZip()
    {
        try
        {
            var files = Directory.GetFiles(Path.Combine(Paths.BepInExRootPath, "plugins"));
            foreach (var file in files)
            {
                if (!file.EndsWith(".zip"))
                    continue;
                var Info = new ModInfo();
                var ModName = Path.GetFileNameWithoutExtension(file);
                var ModDirName = Path.GetFileNameWithoutExtension(file);
                //System.Collections.ObjectModel.ReadOnlyCollection<ZipArchiveEntry> entrys = null;
                ICollection<ZipEntry> entrys;

                //Check if is a Mod Directory and Load Mod Info
                try
                {
                    //zip = ZipFile.Open(file, ZipArchiveMode.Read);
                    var zip = ZipFile.Read(file);
                    entrys = zip.Entries;
                    ModDirName = entrys.ElementAt(0).FileName.Substring(0, entrys.ElementAt(0).FileName.Length - 1);

                    //var ModInfoZip = zip.GetEntry(ModDirName + @"/ModInfo.json");
                    var ModInfoZip = zip[ModDirName + @"/ModInfo.json"];
                    if (ModInfoZip == null)
                        continue;

                    // Load Mod Info
                    var ms = new MemoryStream();
                    ModInfoZip.Extract(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(ms))
                    {
                        JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), Info);
                    }

                    if (Info.ModEditorVersion.IsNullOrWhiteSpace())
                        continue;

                    // Check Name
                    if (!Info.Name.IsNullOrWhiteSpace())
                        ModName = Info.Name;

                    if (!ModPacks.ContainsKey(ModName))
                        ModPacks[ModName] = new ModPack(Info, ModName,
                            ModLoaderInstance.Config.Bind("是否加载某个模组",
                                $"{ModName}_{Info.Name}".EscapeStr(), true,
                                $"是否加载{ModName}"), true);
                    else if (ModPacks[ModName].Loaded)
                    {
                        continue;
                    }
                    else
                    {
                        ModPacks[ModName].Loaded = true;
                    }

                    if (!ModPacks[ModName].EnableEntry.Value) continue;

                    Debug.Log($"ModLoader Load EditorZipMod {ModName} {Info.Version}");

                    // Check Verison
                    var ModRequestVersion = Version.Parse(Info.ModLoaderVerison);
                    if (PluginVersion.CompareTo(ModRequestVersion) < 0)
                        Debug.LogWarningFormat(
                            "ModLoader Version {0} is lower than {1} Request Version {2}", PluginVersion, ModName,
                            ModRequestVersion);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("LoadModsFromZip " + ex.Message);
                    continue;
                }

                // Load Resource
                try
                {
                    foreach (var entry in entrys)
                    {
                        if (!(entry.FileName.StartsWith(ModDirName + @"/Resource") &&
                              entry.FileName.EndsWith(".ab")))
                            continue;
                        var ms = new MemoryStream();
                        entry.Extract(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        var ab = AssetBundle.LoadFromStream(ms);
                        foreach (var obj in ab.LoadAllAssets())
                        {
                            if (obj is Sprite sprite)
                            {
                                if (!SpriteDict.ContainsKey(sprite.name))
                                    SpriteDict.Add(sprite.name, sprite);
                                else
                                    Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}",
                                        ModName, sprite.name);
                            }

                            if (obj is AudioClip clip)
                            {
                                if (!AudioClipDict.ContainsKey(clip.name))
                                    AudioClipDict.Add(clip.name, clip);
                                else
                                    Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}",
                                        ModName, clip.name);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load Resource Error {1}", ModName, ex.Message);
                }


                // Load Resource Custom Pictures
                try
                {
                    foreach (var entry in entrys)
                    {
                        if (!(entry.FileName.StartsWith(ModDirName + @"/Resource/Picture") &&
                              (entry.FileName.EndsWith(".jpg") || entry.FileName.EndsWith(".jpeg") ||
                               entry.FileName.EndsWith(".png"))))
                            continue;
                        var sprite_name = Path.GetFileNameWithoutExtension(entry.FileName);
                        var t2d = new Texture2D(0, 0, TextureFormat.RGBA32, 0, false);
                        var ms = new MemoryStream();
                        entry.Extract(ms);
                        t2d.LoadImage(ms.ToArray());
                        // var resized = false;
                        // if (t2d.width < t2d.height && t2d.width > MaxTexWidth)
                        // {
                        //     t2d.Resize(MaxTexWidth, t2d.height * MaxTexWidth / t2d.width);
                        //     resized = true;
                        // }
                        //
                        // if (t2d.height < t2d.width && t2d.height > MaxTexWidth)
                        // {
                        //     t2d.Resize(t2d.width * MaxTexWidth / t2d.height, MaxTexWidth);
                        //     resized = true;
                        // }
                        //
                        // if (resized)
                        // {
                        //     t2d.Apply();
                        // }

                        if (!TexCompatibilityMode.Value) t2d.ToCompress();

                        var sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height),
                            Vector2.zero);
                        sprite.name = sprite_name;
                        if (!SpriteDict.ContainsKey(sprite_name))
                            SpriteDict.Add(sprite_name, sprite);
                        else
                            Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}", ModName,
                                sprite_name);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load Resource Custom Pictures Error {1}", ModName,
                        ex.Message);
                }

                // Load Resource Custom Audio
                try
                {
                    foreach (var entry in entrys)
                    {
                        if (!entry.FileName.StartsWith(ModDirName + "/Resource/Audio")) continue;
                        if (entry.FileName.EndsWith(".wav", true, null))
                        {
                            var clip_name = Path.GetFileNameWithoutExtension(entry.FileName);
                            var clip = ResourceDataLoader.GetAudioClipFromWav(entry.OpenReader(), clip_name);
                            if (!clip) continue;
                            if (!AudioClipDict.ContainsKey(clip.name))
                                AudioClipDict.Add(clip.name, clip);
                            else
                                Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}",
                                    ModName, clip.name);
                        }
                        else if (entry.FileName.EndsWith(".mp3", true, null))
                        {
                            var clip_name = Path.GetFileNameWithoutExtension(entry.FileName);
                            var clip = ResourceDataLoader.GetAudioClipFromMp3(entry.OpenReader(), clip_name);
                            if (!clip) continue;
                            if (!AudioClipDict.ContainsKey(clip.name))
                                AudioClipDict.Add(clip.name, clip);
                            else
                                Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}",
                                    ModName, clip.name);
                        }
                        else if (entry.FileName.EndsWith(".ogg", true, null))
                        {
                            var clip_name = Path.GetFileNameWithoutExtension(entry.FileName);
                            var clip = ResourceDataLoader.GetAudioClipFromOgg(entry.OpenReader(), clip_name);
                            if (!clip) continue;
                            if (!AudioClipDict.ContainsKey(clip.name))
                                AudioClipDict.Add(clip.name, clip);
                            else
                                Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}",
                                    ModName, clip.name);
                        }
                        //MBSingleton<GameLoad>.Instance.StartCoroutine(GetDataRequest(ModName, file));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load Resource Custom Audio Error {1}", ModName,
                        ex.Message);
                }

                // Load ScriptableObject
                try
                {
                    var subclasses = from type in AccessTools.AllTypes()
                        where type.IsSubclassOf(typeof(ScriptableObject))
                        select type;

                    foreach (var type in subclasses)
                    {
                        if (!AllScriptableObjectWithoutGuidTypeDict.ContainsKey(type))
                            AllScriptableObjectWithoutGuidTypeDict[type] =
                                new Dictionary<string, ScriptableObject>();

                        if (type.IsSubclassOf(typeof(UniqueIDScriptable)) || type == typeof(UniqueIDScriptable))
                            continue;
                        foreach (var entry in entrys)
                        {
                            if (!(entry.FileName.StartsWith(ModDirName + @"/ScriptableObject/" + type.Name) &&
                                  entry.FileName.EndsWith(".json")))
                                continue;
                            var obj_name = Path.GetFileNameWithoutExtension(entry.FileName);
                            if (AllScriptableObjectWithoutGuidTypeDict.TryGetValue(type, out var dict))
                            {
                                if (dict.ContainsKey(obj_name))
                                    continue;
                                string CardData;

                                var obj = ScriptableObject.CreateInstance(type);
                                var ms = new MemoryStream();
                                entry.Extract(ms);
                                ms.Seek(0, SeekOrigin.Begin);
                                using (var sr = new StreamReader(ms))
                                {
                                    CardData = sr.ReadToEnd();
                                }

                                obj.name = obj_name;
                                var jsonData = JsonMapper.ToObject(CardData);
                                if (obj is IModLoaderJsonObj modLoaderJsonObj)
                                {
                                    modLoaderJsonObj.CreateByJson(CardData);
                                }
                                else
                                {
                                    JsonUtility.FromJsonOverwrite(CardData, obj);
                                }
                                dict.Add(obj_name, obj);
                                WaitForWarpperEditorNoGuidList.Add(new ScriptableObjectPack(obj,
                                    "", "", ModName, new JsonKVProvider(jsonData)));
                                if (!AllScriptableObjectDict.ContainsKey(obj_name))
                                    AllScriptableObjectDict.Add(obj_name, obj);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load ScriptableObject Error {1}", ModName, ex.Message);
                }

                // Load Localization
                try
                {
                    foreach (var entry in entrys)
                    {
                        if (!(entry.FileName.StartsWith(ModDirName + @"/Localization") &&
                              entry.FileName.EndsWith(".csv")))
                            continue;
                        var ms = new MemoryStream();
                        entry.Extract(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        using (var sr = new StreamReader(ms))
                        {
                            WaitForLoadCSVList.Add((Path.GetFileName(entry.FileName),
                                sr.ReadToEnd()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load Localization Error {1}", ModName, ex.Message);
                }

                // Load and init UniqueIDScriptable
                try
                {
                    var subclasses = from type in GameSourceAssembly.GetTypes()
                        where type.IsSubclassOf(typeof(UniqueIDScriptable))
                        select type;

                    foreach (var type in subclasses)
                    foreach (var entry in entrys)
                    {
                        if (!(entry.FileName.StartsWith(ModDirName + @"/" + type.Name) &&
                              entry.FileName.EndsWith(".json")))
                            continue;
                        var CardName = Path.GetFileNameWithoutExtension(entry.FileName);
                        try
                        {
                            JsonData json;
                            var ms = new MemoryStream();
                            entry.Extract(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            string CardData;
                            using (var sr = new StreamReader(ms))
                            {
                                CardData = sr.ReadToEnd();
                                json = JsonMapper.ToObject(CardData);
                            }

                            if (!(json.ContainsKey("UniqueID") && json["UniqueID"].IsString &&
                                  !json["UniqueID"].ToString().IsNullOrWhiteSpace()))
                            {
                                Debug.LogErrorFormat(
                                    "{0} EditorLoadZip {1} {2} try to load a UniqueIDScriptable without GUID",
                                    type.Name, ModName, CardName);
                                continue;
                            }

                            var card = ScriptableObject.CreateInstance(type) as UniqueIDScriptable;
                            // JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                            if (card is IModLoaderJsonObj modLoaderJsonObj)
                            {
                                modLoaderJsonObj.CreateByJson(CardData);
                            }
                            else
                            {
                                JsonUtility.FromJsonOverwrite(CardData, card);
                            }

                            card.name = $"{ModName}_{CardName}";

                            //type.GetMethod("Init", bindingFlags, null, new Type[] { }, null).Invoke(card, null);
                            var card_guid = card.UniqueID;
                            AllGUIDDict.Add(card_guid, card);
                            GameLoad.Instance.DataBase.AllData.Add(card);

                            if (!WaitForWarpperEditorGuidDict.ContainsKey(card_guid))
                                WaitForWarpperEditorGuidDict.Add(card_guid,
                                    new ScriptableObjectPack(card, "", "", ModName,
                                        new JsonKVProvider(json)));
                            else
                                Debug.LogWarningFormat(
                                    "{0} WaitForWarpperEditorGuidDict Same Key was Add {1}", ModName,
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
                                ModName, CardName, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load UniqueIDScriptable Error {1}", ModName,
                        ex.Message);
                }

                // Load GameSourceModify
                try
                {
                    //var modify_dirs = Directory.GetDirectories(CombinePaths(dir, "GameSourceModify"));
                    foreach (var entry in entrys)
                    {
                        if (!(entry.FileName.StartsWith(ModDirName + @"/GameSourceModify") &&
                              entry.FileName.EndsWith(".json")))
                            continue;
                        string CardData;
                        var Guid = Path.GetFileNameWithoutExtension(entry.FileName);
                        var ms = new MemoryStream();
                        entry.Extract(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        using (var sr = new StreamReader(ms))
                        {
                            CardData = sr.ReadToEnd();
                        }

                        var jsonData = JsonMapper.ToObject(CardData);
                        WaitForWarpperEditorGameSourceGUIDList.Add(
                            AllGUIDDict.TryGetValue(Guid, out var obj)
                                ? new ScriptableObjectPack(obj, "", "", ModName, new JsonKVProvider(jsonData))
                                : new ScriptableObjectPack(null, Guid, "", ModName, new JsonKVProvider(jsonData)));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load GameSourceModify Error {1}", ModName, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    private static readonly List<Task<(List<(byte[] dat, string name)> sprites, string modName)>> spritesWaitList =
        new();

    public static readonly List<Task<(List<(byte[] dat, string pat, Type type)> uniqueObjs, string modName)>>
        uniqueObjWaitList = new();

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

                var Info = new ModInfo();
                var ModName = Path.GetFileName(dir);

                try
                {
                    // Load Mod Info
                    using (var sr = new StreamReader(CombinePaths(dir, "ModInfo.json")))
                    {
                        JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), Info);
                    }

                    // Check Name
                    if (!Info.Name.IsNullOrWhiteSpace())
                        ModName = Info.Name;

                    if (!ModPacks.ContainsKey(ModName))
                        ModPacks[ModName] = new ModPack(Info, ModName,
                            ModLoaderInstance.Config.Bind("是否加载某个模组",
                                $"{ModName}_{Info.Name}".EscapeStr(), true,
                                $"是否加载{ModName}"), true);
                    else
                    {
                        ModPacks[ModName].Loaded = true;
                    }

                    if (!ModPacks[ModName].EnableEntry.Value) continue;

                    Debug.Log($"ModLoader Load Mod {ModName} {Info.Version}");

                    // Check Verison
                    var ModRequestVersion = Version.Parse(Info.ModLoaderVerison);
                    if (PluginVersion.CompareTo(ModRequestVersion) < 0)
                        Debug.LogWarningFormat(
                            "ModLoader Version {0} is lower than {1} Request Version {2}", PluginVersion, ModName,
                            ModRequestVersion);
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Check Version Error {1}", ModName, ex.Message);
                }

                // Load Resource
                try
                {
                    if (Directory.Exists(CombinePaths(dir, "Resource")))
                    {
                        var files = Directory.GetFiles(CombinePaths(dir, "Resource"));
                        foreach (var file in files)
                        {
                            if (!file.EndsWith(".ab")) continue;
                            var ab = AssetBundle.LoadFromFile(file);
                            foreach (var obj in ab.LoadAllAssets())
                            {
                                if (obj is Sprite sprite)
                                {
                                    if (!SpriteDict.ContainsKey(sprite.name))
                                        SpriteDict.Add(sprite.name, sprite);
                                    else
                                        Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}",
                                            ModName, sprite.name);
                                }

                                if (obj is AudioClip clip)
                                {
                                    if (!AudioClipDict.ContainsKey(clip.name))
                                        AudioClipDict.Add(clip.name, clip);
                                    else
                                        Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}",
                                            ModName, clip.name);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load Resource Error {1}", ModName, ex.Message);
                }

                // Load Resource Custom Audio
                try
                {
                    if (Directory.Exists(CombinePaths(dir, "Resource", "Audio")))
                    {
                        var files = Directory.GetFiles(CombinePaths(dir, "Resource", "Audio"));
                        foreach (var file in files)
                            if (file.EndsWith(".wav", true, null))
                            {
                                var raw_data = File.Open(file, FileMode.Open);
                                var clip_name = Path.GetFileNameWithoutExtension(file);
                                var clip = ResourceDataLoader.GetAudioClipFromWav(raw_data, clip_name);
                                if (!clip) continue;
                                if (!AudioClipDict.ContainsKey(clip.name))
                                    AudioClipDict.Add(clip.name, clip);
                                else
                                    Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}",
                                        ModName, clip.name);
                            }
                            else if (file.EndsWith(".mp3", true, null))
                            {
                                var raw_data = File.Open(file, FileMode.Open);
                                var clip_name = Path.GetFileNameWithoutExtension(file);
                                var clip = ResourceDataLoader.GetAudioClipFromMp3(raw_data, clip_name);
                                if (!clip) continue;
                                if (!AudioClipDict.ContainsKey(clip.name))
                                    AudioClipDict.Add(clip.name, clip);
                                else
                                    Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}",
                                        ModName, clip.name);
                            }
                            else if (file.EndsWith(".ogg", true, null))
                            {
                                var raw_data = File.Open(file, FileMode.Open);
                                var clip_name = Path.GetFileNameWithoutExtension(file);
                                var clip = ResourceDataLoader.GetAudioClipFromOgg(raw_data, clip_name);
                                if (!clip) continue;
                                if (!AudioClipDict.ContainsKey(clip.name))
                                    AudioClipDict.Add(clip.name, clip);
                                else
                                    Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}",
                                        ModName, clip.name);
                            }
                        //MBSingleton<GameLoad>.Instance.StartCoroutine(GetDataRequest(ModName, file));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load Resource Custom Audio Error {1}", ModName,
                        ex.Message);
                }

                // Load ScriptableObject
                try
                {
                    if (Directory.Exists(CombinePaths(dir, "ScriptableObject")))
                    {
                        var subclasses = from type in AccessTools.AllTypes()
                            where type.IsSubclassOf(typeof(ScriptableObject))
                            select type;

                        foreach (var type in subclasses)
                        {
                            if (!AllScriptableObjectWithoutGuidTypeDict.ContainsKey(type))
                                AllScriptableObjectWithoutGuidTypeDict[type] =
                                    new Dictionary<string, ScriptableObject>();

                            if (type.IsSubclassOf(typeof(UniqueIDScriptable)) || type == typeof(UniqueIDScriptable))
                                continue;

                            if (!Directory.Exists(CombinePaths(dir, "ScriptableObject", type.Name)))
                                continue;

                            foreach (var file in Directory.EnumerateFiles(
                                         CombinePaths(dir, "ScriptableObject", type.Name), "*.json",
                                         SearchOption.AllDirectories))
                            {
                                var obj_name = Path.GetFileNameWithoutExtension(file);
                                if (AllScriptableObjectWithoutGuidTypeDict.TryGetValue(type, out var dict))
                                {
                                    if (dict.ContainsKey(obj_name))
                                        continue;
                                    string CardData;

                                    var obj = ScriptableObject.CreateInstance(type);
                                    using (var sr = new StreamReader(file))
                                    {
                                        CardData = sr.ReadToEnd();
                                    }

                                    obj.name = obj_name;
                                    var jsonData = JsonMapper.ToObject(CardData);
                                    if (obj is IModLoaderJsonObj modLoaderJsonObj)
                                    {
                                        modLoaderJsonObj.CreateByJson(CardData);
                                    }
                                    else
                                    {
                                        JsonUtility.FromJsonOverwrite(CardData, obj);
                                    }
                                    dict.Add(obj_name, obj);
                                    WaitForWarpperEditorNoGuidList.Add(
                                        new ScriptableObjectPack(obj, "", "", ModName,
                                            new JsonKVProvider(jsonData)));
                                    if (!AllScriptableObjectDict.ContainsKey(obj_name))
                                        AllScriptableObjectDict.Add(obj_name, obj);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load ScriptableObject Error {1}", ModName, ex.Message);
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
                            using (var sr = new StreamReader(file))
                            {
                                WaitForLoadCSVList.Add((Path.GetFileName(file),
                                    sr.ReadToEnd()));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load Localization Error {1}", ModName, ex.Message);
                }

                /*
                // Load and init UniqueIDScriptable
                try
                {
                    uniqueObjWaitList.Add(
                        ResourceLoadHelper.LoadUniqueObjs(ModName, dir, GameSrouceAssembly, Info));
                    // var subclasses = from type in GameSrouceAssembly.GetTypes()
                    //     where type.IsSubclassOf(typeof(UniqueIDScriptable))
                    //     select type;
                    //
                    // foreach (var type in subclasses)
                    // {
                    //     if (!Directory.Exists(CombinePaths(dir, type.Name)))
                    //         continue;
                    //     if (Info.ModEditorVersion.IsNullOrWhiteSpace())
                    //     {
                    //         Debug.LogWarningFormat("{0} Only Support Editor Mod", ModName);
                    //     }
                    //     else
                    //     {
                    //         foreach (var file in Directory.EnumerateFiles(CombinePaths(dir, type.Name), "*.json",
                    //                      SearchOption.AllDirectories))
                    //         {
                    //             string CardName = Path.GetFileNameWithoutExtension(file);
                    //             try
                    //             {
                    //                 JsonData json;
                    //                 var CardData = "";
                    //                 using (StreamReader sr = new StreamReader(file))
                    //                 {
                    //                     CardData = sr.ReadToEnd();
                    //                     json = JsonMapper.ToObject(CardData);
                    //                 }
                    //
                    //                 if (!(json.ContainsKey("UniqueID") && json["UniqueID"].IsString &&
                    //                       !json["UniqueID"].ToString().IsNullOrWhiteSpace()))
                    //                 {
                    //                     Debug.LogErrorFormat(
                    //                         "{0} EditorLoadZip {1} {2} try to load a UniqueIDScriptable without GUID",
                    //                         type.Name, ModName, CardName);
                    //                     continue;
                    //                 }
                    //
                    //                 var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic |
                    //                                    BindingFlags.Public;
                    //                 var card = type
                    //                     .GetMethod("CreateInstance",
                    //                         bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy,
                    //                         null, new Type[] {typeof(Type)}, null)
                    //                     .Invoke(null, new object[] {type});
                    //                 JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                    //                 JsonUtility.FromJsonOverwrite(CardData, card);
                    //
                    //                 type.GetProperty("name", bindingFlags).GetSetMethod(true)
                    //                     .Invoke(card, new object[] {ModName + "_" + CardName});
                    //                 //type.GetMethod("Init", bindingFlags, null, new Type[] { }, null).Invoke(card, null);
                    //
                    //                 var card_guid =
                    //                     type.GetField("UniqueID", bindingFlags).GetValue(card) as string;
                    //                 AllGUIDDict.Add(card_guid, card as UniqueIDScriptable);
                    //                 GameLoad.Instance.DataBase.AllData.Add(card as UniqueIDScriptable);
                    //
                    //                 if (!WaitForWarpperEditorGuidDict.ContainsKey(card_guid))
                    //                     WaitForWarpperEditorGuidDict.Add(card_guid,
                    //                         new ScriptableObjectPack(card as UniqueIDScriptable, "", file,
                    //                             ModName, CardData));
                    //                 else
                    //                     Debug.LogWarningFormat(
                    //                         "{0} WaitForWarpperEditorGuidDict Same Key was Add {1}", ModName,
                    //                         card_guid);
                    //                 if (!AllScriptableObjectDict.ContainsKey(card_guid))
                    //                     AllScriptableObjectDict.Add(card_guid, card as ScriptableObject);
                    //                 if (AllGUIDTypeDict.TryGetValue(type, out var dict))
                    //                     if (!dict.ContainsKey(card_guid))
                    //                         dict.Add(card_guid, card as UniqueIDScriptable);
                    //             }
                    //             catch (Exception ex)
                    //             {
                    //                 Debug.LogWarningFormat("{0} EditorLoad {1} {2} Error {3}",
                    //                     type.Name, ModName, CardName, ex.Message);
                    //             }
                    //         }
                    //     }
                    // }
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load UniqueIDScriptable Error {1}", ModName,
                        ex.Message);
                }*/

                // Load GameSourceModify
                try
                {
                    if (Info.ModEditorVersion.IsNullOrWhiteSpace())
                    {
                        Debug.LogWarningFormat("{0} Only Support Editor Mod", ModName);
                    }
                    else
                    {
                        if (Directory.Exists(CombinePaths(dir, "GameSourceModify")))
                            foreach (var file in Directory.EnumerateFiles(CombinePaths(dir, "GameSourceModify"),
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
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("{0} Load GameSourceModify Error {1}", ModName, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }


    private static void LoadEditorScriptableObject()
    {
        foreach (var item in WaitForWarpperEditorNoGuidList)
        {
            try
            {
                ProcessingScriptableObjectPack = item;
                var json = item.CardData;
                if (json == null) continue;
                WarpperFunction.JsonCommonWarpper(item.obj, json);
                if (item.obj is CardTabGroup && item.obj.name.StartsWith("Tab_"))
                    WaitForAddCardTabGroup.Add(new ScriptableObjectPack(item.obj, "", "", "", item.CardData));

                if (item.obj is ContentPage)
                {
                    if (item.obj.name.EndsWith("Default"))
                        WaitForAddDefaultContentPage.Add(new ScriptableObjectPack(item.obj, "", "", "",
                            item.CardData));
                    else if (item.obj.name.EndsWith("Main"))
                        WaitForAddMainContentPage.Add(new ScriptableObjectPack(item.obj, "", "", "",
                            item.CardData));
                }

                if (item.obj is GuideEntry entry) WaitForAddGuideEntry.Add(entry);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("LoadEditorScriptableObject " + ex.Message);
            }
        }
    }

    public static readonly SimpleOnce _onceWarp = new();

    private static void LoadLocalization()
    {
        var regex = new Regex(@"\\n");
        if (LocalizationManager.Instance.Languages[LocalizationManager.CurrentLanguage].LanguageName == "简体中文")
            foreach (var pair in WaitForLoadCSVList)
                try
                {
                    if (pair.Item1.Contains("SimpCn"))
                    {
                        var CurrentTexts = CurrentTextsFieldRef();
                        var dictionary = CSVParser.LoadFromString(pair.Item2);
                        foreach (var keyValuePair in dictionary)
                            if (!CurrentTexts.ContainsKey(keyValuePair.Key) && keyValuePair.Value.Count >= 2)
                            {
                                var ChLocal = regex.Replace(keyValuePair.Value[1], "\n");
                                if (!ChLocal.Trim().IsNullOrWhiteSpace()) CurrentTexts.Add(keyValuePair.Key, ChLocal);
                            }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("LoadLocalization " + ex.Message);
                }

        if (LocalizationManager.Instance.Languages[LocalizationManager.CurrentLanguage].LanguageName == "English")
            foreach (var pair in WaitForLoadCSVList)
                try
                {
                    if (pair.Item1.Contains("SimpEn"))
                    {
                        var CurrentTexts = CurrentTextsFieldRef();
                        var dictionary = CSVParser.LoadFromString(pair.Item2);
                        foreach (var keyValuePair in dictionary)
                            if (!CurrentTexts.ContainsKey(keyValuePair.Key) && keyValuePair.Value.Count >= 2)
                            {
                                var EnLocal = regex.Replace(keyValuePair.Value[0], "\n");
                                if (!EnLocal.Trim().IsNullOrWhiteSpace()) CurrentTexts.Add(keyValuePair.Key, EnLocal);
                            }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("LoadLocalization " + ex.Message);
                }
    }

    private static void AddBlueprintCardData(GraphicsManager instance)
    {
        foreach (var tuple in WaitForAddBlueprintCard)
            try
            {
                foreach (var group in instance.BlueprintModelsPopup.BlueprintTabs)
                    if (group.name == tuple.Item1)
                    {
                        group.ShopSortingList.Add(tuple.Item3);
                        foreach (var sub_group in group.SubGroups)
                            if (sub_group.name == tuple.Item2)
                            {
                                sub_group.IncludedCards.Add(tuple.Item3);
                                break;
                            }

                        break;
                    }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("AddBlueprintCardData " + ex.Message);
            }
    }

    private static void AddVisibleGameStat(GraphicsManager instance)
    {
        foreach (var tuple in WaitForAddVisibleGameStat)
            try
            {
                // var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                // var StatList =
                //     instance.AllStatsList.GetType().GetField("Tabs", bindingFlags)
                //         .GetValue(instance.AllStatsList) as StatListTab[];
                var StatList = Traverse.Create(instance.AllStatsList).Field<StatListTab[]>("Tabs").Value;
                foreach (var list in StatList)
                    if (list.name == tuple.Item1)
                    {
                        list.ContainedStats.Add(tuple.Item2);
                        break;
                    }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("AddVisibleGameStat " + ex.Message);
            }
    }

    private static void AddPerkGroup()
    {
        foreach (var tuple in WaitForAddPerkGroup)
            try
            {
                if (AllGUIDTypeDict.TryGetValue(typeof(PerkGroup), out var dict))
                    if (dict.TryGetValue(tuple.Item1, out var group))
                    {
                        var obj = group as PerkGroup;
                        if (obj)
                        {
                            Array.Resize(ref obj.PerksList, obj.PerksList.Length + 1);
                            obj.PerksList[obj.PerksList.Length - 1] = tuple.Item2;
                        }
                    }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("AddPerkGroup " + ex.Message);
            }
    }

    private static void AddCardTabGroup(GraphicsManager instance)
    {
        foreach (var item in WaitForAddCardTabGroup)
            try
            {
                if (!(item.obj is CardTabGroup tabGroup))
                    continue;

                tabGroup.FillSortingList();

                if (!tabGroup.name.StartsWith("Tab_"))
                    continue;

                if (tabGroup.SubGroups.Count != 0)
                {
                    Array.Resize(ref instance.BlueprintModelsPopup.BlueprintTabs,
                        instance.BlueprintModelsPopup.BlueprintTabs.Length + 1);
                    instance.BlueprintModelsPopup.BlueprintTabs[
                        instance.BlueprintModelsPopup.BlueprintTabs.Length - 1] = tabGroup;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("AddCardTabGroup " + ex.Message);
            }
    }

    private static void AddCardTabGroupOnce(GraphicsManager instance)
    {
        foreach (var item in WaitForAddCardTabGroup)
            try
            {
                if (item.obj is not CardTabGroup itemObj)
                    continue;

                itemObj.FillSortingList();

                if (!itemObj.name.StartsWith("Tab_"))
                    continue;

                if (itemObj.SubGroups.Count == 0)
                {
                    var json = item.CardData;
                    if (json.ContainsKey("BlueprintCardDataCardTabGroup") &&
                        json["BlueprintCardDataCardTabGroup"].IsString && !json["BlueprintCardDataCardTabGroup"]
                            .ToString().IsNullOrWhiteSpace())
                        foreach (var group in instance.BlueprintModelsPopup.BlueprintTabs)
                            if (group.name == json["BlueprintCardDataCardTabGroup"].ToString())
                            {
                                group.SubGroups.Add(itemObj);
                                group.FillSortingList();
                                break;
                            }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("AddCustomCardTabGroup " + ex.Message);
            }
    }

    private static void AddCardFilterGroupOnce()
    {
        var CardFilterGroupDict = new Dictionary<string, CardFilterGroup>();

        foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(CardFilterGroup)))
        {
            if (ele.GetType().Assembly != GameSourceAssembly)
                continue;
            CardFilterGroupDict.Add(ele.name, ele as CardFilterGroup);
        }

        foreach (var item in WaitForAddCardFilterGroupCard)
            if (CardFilterGroupDict.TryGetValue(item.Item1, out var Filter))
                Filter.IncludedCards.Add(item.Item2);
    }

    private static IEnumerator FXMaskPostAwake(FXMask instance)
    {
        while (!AmbienceImageEffect.Instance) yield return null;

        var tInstance = Traverse.Create(instance);
        tInstance.Field<RectTransform>("MyRectTr").Value = instance.GetComponent<RectTransform>();
        tInstance.Field<AmbienceImageEffect>("AmbienceEffects").Value = AmbienceImageEffect.Instance;
        var fieldMaskObject = tInstance.Field<SpriteMask>("MaskObject");
        fieldMaskObject.Value = Instantiate(AmbienceImageEffect.Instance.MaskPrefab,
            AmbienceImageEffect.Instance.WeatherEffectsParent);
        if (GameManager.DontRenameGOs)
            yield break;
        fieldMaskObject.Value.name = instance.name + "_Mask";
    }

    public class MyScriptableObject : ScriptableObject
    {
        public int aaa;
    }

    private static bool FixFXMaskAwake(FXMask __instance)
    {
        if (!AmbienceImageEffect.Instance)
        {
            __instance.StartCoroutine(FXMaskPostAwake(__instance));
            return false;
        }

        return true;
    }

    private static IEnumerator WaiterForContentDisplayer()
    {
        var done = false;
        while (true)
        {
            var objs = Resources.FindObjectsOfTypeAll<ContentDisplayer>();

            foreach (var obj in objs)
            {
                if (obj.gameObject.name != "JournalTourist") continue;
                ContentDisplayer displayer = null;
                GameObject clone = null;
                try
                {
                    clone = Instantiate(obj.gameObject);
                    displayer = clone.GetComponent<ContentDisplayer>();
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

            if (done) break;

            yield return new WaitForSeconds(0.5f);
        }

        var displayers = Resources.FindObjectsOfTypeAll(typeof(ContentDisplayer));
        foreach (var displayer in displayers)
            try
            {
                if (!CustomContentDisplayerDict.ContainsKey(displayer.name))
                    CustomContentDisplayerDict.Add(displayer.name, displayer as ContentDisplayer);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("CustomContentDisplayerDict Warning " + ex.Message);
            }

        while (!_onceWarp.Done()) yield return null;

        foreach (var item in WaitForAddDefaultContentPage)
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
                        clone = Instantiate(sample);
                        displayer = clone.GetComponent(typeof(ContentDisplayer)) as ContentDisplayer;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("FXMask Warning " + ex.Message);
                    }

                    if (displayer == null) continue;

                    var modPage = item.obj as ContentPage;
                    if (modPage == null) continue;

                    var tDisplayer = Traverse.Create(displayer);
                    var pages = tDisplayer.Field<List<ContentPage>>("ExplicitPageContent").Value;
                    pages.Clear();
                    pages.Add(modPage);
                    tDisplayer.Field<ContentPage>("DefaultPage").Value = modPage;

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

        foreach (var item in WaitForAddMainContentPage)
            try
            {
                var name_parts = item.obj.name.Split('_');
                if (name_parts.Length > 2)
                    if (CustomContentDisplayerDict.TryGetValue(name_parts[0] + "_" + name_parts[1],
                            out var displayer))
                    {
                        var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                        var pages = typeof(ContentDisplayer).GetField("ExplicitPageContent", bindingFlags)
                            .GetValue(displayer) as IList;
                        pages.Add(item.obj);
                    }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("WaiterForContentDisplayer WaitForAddMainContentPage " + ex.Message);
            }

        foreach (var item in WaitForAddJournalPlayerCharacter)
            try
            {
                if (item.obj is not PlayerCharacter character)
                    continue;

                var json = item.CardData;
                if (json.ContainsKey("PlayerCharacterJournalName") && json["PlayerCharacterJournalName"].IsString &&
                    !json["PlayerCharacterJournalName"].ToString().IsNullOrWhiteSpace())
                    if (CustomContentDisplayerDict.TryGetValue(json["PlayerCharacterJournalName"].ToString(),
                            out var displayer))
                        character.Journal = displayer;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("WaiterForContentDisplayer PlayerCharacterJournalName " + ex.Message);
            }
    }

    private static void CustomGameObjectFixed()
    {
        foreach (var item in CustomGameObjectListDict)
            try
            {
                var transform = item.Value.transform.Find("Shadow/GuideFrame/GuideContentPage/Content/Horizontal");
                if (transform != null)
                    for (var i = 0; i < transform.childCount; i++)
                        Destroy(transform.GetChild(i).gameObject);

                transform = item.Value.transform.Find("Shadow/GuideFrame");
                var fx = transform.gameObject.GetComponent(typeof(FXMask)) as FXMask;
                if (fx != null) fx.enabled = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("CustomGameObjectFixed " + ex.Message);
            }
    }

    private static void AddPlayerCharacter(GuideManager instance)
    {
        try
        {
            instance.StartCoroutine(WaiterForContentDisplayer());
        }
        catch (Exception ex)
        {
            Debug.LogWarning("AddPlayerCharacter" + ex.Message);
        }
    }

    private static void LoadGuideEntry(GuideManager instance)
    {
        try
        {
            foreach (var entry in WaitForAddGuideEntry) instance.AllEntries.Add(entry);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("LoadGuideEntry" + ex.Message);
        }
    }

    // Patch

    private static readonly SimpleOnce _once = new();

    public static void UniqueIDScriptableClearDictPrefix()
    {
        if (!_once.DoOnce()) return;

        try
        {
            // if (HasEncounterType)
            // {
            //     TestCardAdd.AddTestCard();
            // }
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            LoadGameResource();

            LoadArchMod.LoadAllArchMod();

            LoadMods(Path.Combine(Paths.BepInExRootPath, "plugins"));

            LoadModsFromZip();
            PostSpriteLoad.BeginCompress = true;

            LoadPreData.LoadFromPreLoadData();

            LoadEditorScriptableObject();

            // LoadLocalization();

            var stopwatch1 = new Stopwatch();
            stopwatch1.Start();
            DoWarpperLoader.WarpperAllEditorMods();

            DoWarpperLoader.WarpperAllEditorGameSrouces();

            DoWarpperLoader.MatchAndWarpperAllEditorGameSrouce();

            stopwatch1.Stop();
            Debug.LogWarning($"warp time taken:{stopwatch1.Elapsed}");

            AddPerkGroup();

            //AddPlayerCharacter(__instance);

            stopwatch.Stop();
            Debug.Log("ModLoader Time taken: " + stopwatch.Elapsed);
            _once.SetDone();
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
        finally
        {
            PostSpriteLoad.CanEnd = true;
        }
    }

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

    public static void GuideManagerStartPrefix(GuideManager __instance)
    {
        try
        {
            LoadGuideEntry(__instance);

            AddPlayerCharacter(__instance);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    private static bool init_flag;

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

                AddCardFilterGroupOnce();
                init_flag = true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    public static bool ModManagerUIOn;
    public static Vector2 ModManagerUIScrollViewPos;

    public static Rect ModManagerUIWindowRect = new(Screen.width * 0.12f, Screen.height * 0.12f,
        Screen.width * 0.55f,
        Screen.height * 0.55f);

    public static Rect LoadSuccessUIWindowRect = new(Screen.width * 0.45f, Screen.height * 0.45f,
        Screen.width * 0.1f,
        Screen.height * 0.1f);

    public static Rect UpdateSuccessUIWindowRect = new(Screen.width * 0.4f, Screen.height * 0.4f,
        Screen.width * 0.2f,
        Screen.height * 0.2f);

    public static bool ReqQuit;
    public static float WaitTime;
    public static bool HadBootNew;

    private void Update()
    {
        if (ReqQuit)
        {
            if (WaitTime > 0.5)
            {
                WaitTime -= Time.deltaTime;
            }
            else if (WaitTime > 0)
            {
                if (!HadBootNew)
                {
                    HadBootNew = true;
                    Process.Start("explorer.exe steam://rungameid/1694420");
                }

                WaitTime -= Time.deltaTime;
            }
            else
            {
                Application.Quit();
            }
        }

        if (Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.LeftCommand) ||
            Input.GetKey(KeyCode.RightControl) ||
            Input.GetKey(KeyCode.RightCommand))
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                ModManagerUIOn = !ModManagerUIOn;
                MainUIBackPanelRT.gameObject.SetActive(ModManagerUIOn);
            }
    }

    public static float ShowLoadSuccess;

    public static GUIStyle bigLabel;
    public static int CurrentMainUIId;
    public static Vector2 CurrentMainUIIdSelectScroll;

    private void OnGUI()
    {
        bigLabel ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 32
        };
        if (ShowLoadSuccess > 0)
        {
            ShowLoadSuccess -= Time.deltaTime;
            GUILayout.Window(0x893ffa, LoadSuccessUIWindowRect, PostLoadSuccessWindow, "PostLoadSuccess");
        }

        if (ModManagerUIOn)
            GUILayout.Window(CurrentMainUIId, ModManagerUIWindowRect,
                ModLoaderMainUIWindow, "ModManagerUI");

        if (ModLoaderUpdated)
        {
            GUILayout.Window(0, UpdateSuccessUIWindowRect, UpdateSuccessWindow, "更新完成 UpdateSuccess");

            void UpdateSuccessWindow(int id)
            {
                GUILayout.BeginVertical();

                GUILayout.Label("ModLoader自动更新完成(ModLoader Auto Update Successs)\n是否重启？");
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("重启并激活更新(ReloadGame)"))
                {
                    ReqQuit = true;
                    WaitTime = 2.4f;
                    ModLoaderUpdated = false;
                }

                if (GUILayout.Button("暂不重启(No)")) ModLoaderUpdated = false;

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
        }
    }

    private static void PostLoadSuccessWindow(int id)
    {
        GUILayout.BeginVertical();
        GUILayout.Label("加载正式完成", bigLabel);
        GUILayout.Label("Load All Success", bigLabel);
        GUILayout.EndVertical();
    }

    private static void ModLoaderMainUIWindow(int id)
    {
        CurrentMainUIIdSelectScroll = GUILayout.BeginScrollView(CurrentMainUIIdSelectScroll,
            GUILayout.MaxHeight(ModManagerUIWindowRect.height * 0.08f));
        GUILayout.BeginHorizontal();

        var button0 = GUILayout.Button(id == 0 ? "管理器(Manager)♪(´▽｀)" : "管理器(Manager)");
        if (button0) id = 0;

        var button1 = GUILayout.Button(id == 1 ? "模组导出(Export)♪(´▽｀)" : "模组导出(Export)");
        if (button1) id = 1;

        CurrentMainUIId = id;
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        ModLoaderMainUIWindowById(id);
    }

    private static void ModLoaderMainUIWindowById(int id)
    {
        switch (id)
        {
            case 0:
                ModManagerUIWindow();
                break;
            case 1:
                ExportUI.ModExportUIWindow();
                break;
        }
    }

    private static void ModManagerUIWindow()
    {
        GUILayout.BeginVertical();
        TexCompatibilityMode.Value =
            GUILayout.Toggle(TexCompatibilityMode.Value, "是否启用纹理兼容模式（开启后纹理占用内存会增加，请仅在缺图时开启）");
        ModManagerUIScrollViewPos = GUILayout.BeginScrollView(ModManagerUIScrollViewPos);

        foreach (var (key, val) in ModPacks) ModManagerIns(val);

        GUILayout.EndScrollView();

        if (GUILayout.Button("应用(三秒后关闭)/Apply(Three seconds to close)"))
        {
            ModLoaderInstance.Config.Save();
            ReqQuit = true;
            WaitTime = 3;
        }

        GUILayout.EndVertical();
    }

    public static void ModManagerIns(ModPack modPack)
    {
        GUILayout.BeginHorizontal();

        modPack.EnableEntry.Value = GUILayout.Toggle(modPack.EnableEntry.Value, modPack.ModInfo.Name);

        GUILayout.Space(20);

        GUILayout.Label($"文件名/FileName:{modPack.FileName}");

        GUILayout.EndHorizontal();
    }
}