﻿using System;
using BepInEx;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Reflection;

namespace ModLoader
{
    public class ModInfo
    {
        public string Name;
        public string Version;
        public string ModLoaderVerison;
    }

    [BepInPlugin("Dop.plugin.CSTI.ModLoader", "ModLoader", "1.0.4")]
    public class ModLoader : BaseUnityPlugin
    {
        public static System.Version PluginVersion;

        public static Dictionary<string, UnityEngine.Sprite> SpriteDict = new Dictionary<string, UnityEngine.Sprite>();
        public static Dictionary<string, UnityEngine.AudioClip> AudioClipDict = new Dictionary<string, UnityEngine.AudioClip>();
        public static Dictionary<string, WeatherSpecialEffect> WeatherSpecialEffectDict = new Dictionary<string, WeatherSpecialEffect>();

        public static Dictionary<string, UniqueIDScriptable> AllGUIDDict = new Dictionary<string, UniqueIDScriptable>();
        public static Dictionary<string, ScriptableObject> AllScriptableObjectDict = new Dictionary<string, ScriptableObject>();
        public static Dictionary<string, CardTag> CardTagDict = new Dictionary<string, CardTag>();
        public static Dictionary<string, EquipmentTag> EquipmentTagDict = new Dictionary<string, EquipmentTag>();
        public static Dictionary<string, ActionTag> ActionTagDict = new Dictionary<string, ActionTag>();
        public static Dictionary<string, CardTabGroup> CardTabGroupDict = new Dictionary<string, CardTabGroup>();
        public static Dictionary<string, EndgameLogCategory> EndgameLogCategoryDict = new Dictionary<string, EndgameLogCategory>();
        public static Dictionary<string, LocalTickCounter> LocalTickCounterDict = new Dictionary<string, LocalTickCounter>();
        public static Dictionary<string, WeatherSet> WeatherSetDict = new Dictionary<string, WeatherSet>();

        private struct UniqueIDScriptablePack
        {
            public UniqueIDScriptablePack(UniqueIDScriptable obj, string CardDir, string CardPath)
            {
                this.obj = obj;
                this.CardDir = CardDir;
                this.CardPath = CardPath;
            }

            public UniqueIDScriptable obj;
            public string CardDir;
            public string CardPath;
        }

        private static Dictionary<string, UniqueIDScriptablePack> WaitForWarpperGUIDDict = new Dictionary<string, UniqueIDScriptablePack>();
        private static List<UniqueIDScriptablePack> WaitForWarpperGameSourceGUIDList = new List<UniqueIDScriptablePack>();
        private static List<Tuple<string, string>> WaitForLoadCSVList = new List<Tuple<string, string>>();
        private static List<Tuple<string, string,  CardData>> WaitForAddBlueprintCard = new List<Tuple<string, string, CardData>>();
        private static List<Tuple<string, GameStat>> WaitForAddVisibleGameStat = new List<Tuple<string, GameStat>>();

        private void Awake()
        {   
            // Plugin startup logic
            Harmony.CreateAndPatchAll(typeof(ModLoader));
            PluginVersion = System.Version.Parse(this.Info.Metadata.Version.ToString());
            Logger.LogInfo("Plugin ModLoader is loaded! ");
        }

        private static void LoadGameResource()
        {
            foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(ScriptableObject)))
            {
                if (!AllScriptableObjectDict.ContainsKey(ele.name))
                    AllScriptableObjectDict.Add(ele.name, ele as ScriptableObject);

                if (ele is UniqueIDScriptable)
                {
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

            foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Sprite)))
            {
                if (!SpriteDict.ContainsKey(ele.name))
                    SpriteDict.Add(ele.name, ele as UnityEngine.Sprite);
                else
                    UnityEngine.Debug.LogWarning("SpriteDict Same Key was Add " + ele.name);
            }
            foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(UnityEngine.AudioClip)))
            {
                if (!AudioClipDict.ContainsKey(ele.name))
                    AudioClipDict.Add(ele.name, ele as UnityEngine.AudioClip);
                else
                    UnityEngine.Debug.LogWarning("AudioClipDict Same Key was Add " + ele.name);
            }
            foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(WeatherSpecialEffect)))
            {
                if (!WeatherSpecialEffectDict.ContainsKey(ele.name))
                    WeatherSpecialEffectDict.Add(ele.name, ele as WeatherSpecialEffect);
                else
                    UnityEngine.Debug.LogWarning("WeatherSpecialEffectDict Same Key was Add " + ele.name);
            }
        }

        private static void LoadMods()
        {
            try
            {
                var dirs = Directory.GetDirectories(BepInEx.Paths.BepInExRootPath + @"\plugins");
                foreach (var dir in dirs)
                {
                    //  Check if is a Mod Directory
                    if (File.Exists(dir + @"\ModInfo.json"))
                    {
                        ModInfo Info = new ModInfo();
                        string ModeName = Path.GetFileName(dir);

                        try
                        {
                            // Check Verison
                            using (StreamReader sr = new StreamReader(dir + @"\ModInfo.json"))
                                JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), Info);
                            System.Version ModRequestVersion = System.Version.Parse(Info.ModLoaderVerison);
                            if (PluginVersion.CompareTo(ModRequestVersion) < 0)
                                UnityEngine.Debug.LogWarningFormat("ModLoader Version {0} is lower than {1} Request Version {2}", PluginVersion, ModeName, ModRequestVersion);
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogErrorFormat("{0} Check Version Error {1}", ModeName, ex.Message);
                        }

                        // Load Resource
                        try
                        {
                            var files = Directory.GetFiles(dir + @"\Resource");
                            foreach (var file in files)
                            {
                                if (file.EndsWith(".ab"))
                                {
                                    AssetBundle ab = AssetBundle.LoadFromFile(file);
                                    foreach (var obj in ab.LoadAllAssets())
                                    {
                                        if(obj.GetType() == typeof(UnityEngine.Sprite))
                                        {
                                            if (!SpriteDict.ContainsKey(obj.name))
                                                SpriteDict.Add(obj.name, obj as UnityEngine.Sprite);
                                            else
                                                UnityEngine.Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}", ModeName, obj.name);
                                        }
                                        if (obj.GetType() == typeof(UnityEngine.AudioClip))
                                        {
                                            if (!AudioClipDict.ContainsKey(obj.name))
                                                AudioClipDict.Add(obj.name, obj as UnityEngine.AudioClip);
                                            else
                                                UnityEngine.Debug.LogWarningFormat("{0} AudioClipDict Same Key was Add {1}", ModeName, obj.name);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogErrorFormat("{0} Load Resource Error {1}", ModeName, ex.Message);
                        }

                        // Load Resource Custom Pictures
                        try
                        {
                            var files = Directory.GetFiles(dir + @"\Resource\Picture");
                            foreach (var file in files)
                            {
                                if (file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".png"))
                                {
                                    var sprite_name = Path.GetFileNameWithoutExtension(file);
                                    Texture2D t2d = new Texture2D(2, 2);
                                    ImageConversion.LoadImage(t2d, System.IO.File.ReadAllBytes(file));
                                    Sprite sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
                                    if (!SpriteDict.ContainsKey(sprite_name))
                                        SpriteDict.Add(sprite_name, sprite);
                                    else
                                        UnityEngine.Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}", ModeName, sprite_name);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogErrorFormat("{0} Load Resource Custom Pictures Error {1}", ModeName, ex.Message);
                        }

                        // Load Localization
                        try
                        {
                            var files = Directory.GetFiles(dir + @"\Localization");
                            foreach (var file in files)
                            {
                                if (file.EndsWith(".csv"))
                                {
                                    using (StreamReader sr = new StreamReader(file))
                                        WaitForLoadCSVList.Add(new Tuple<string, string>(Path.GetFileName(file), sr.ReadToEnd()));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogErrorFormat("{0} Load Localization Error {1}", ModeName, ex.Message);
                        }

                        // Load and init CardData
                        try
                        {
                            var card_dirs = Directory.GetDirectories(dir + @"\CardData");
                            foreach (var card_dir in card_dirs)
                            {
                                string CardName = Path.GetFileName(card_dir); ;
                                string CardPath = card_dir + @"\" + CardName + ".json";
                                if (File.Exists(CardPath))
                                {
                                    CardData card = CardData.CreateInstance<CardData>();
                                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                                    using (StreamReader sr = new StreamReader(CardPath))
                                        JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), card); 
                                    card.name = CardName;
                                    card.Init();
                                    AllGUIDDict.Add(card.UniqueID, card);
                                    GameLoad.Instance.DataBase.AllData.Add(card);
                                    if(!WaitForWarpperGUIDDict.ContainsKey(card.UniqueID))
                                        WaitForWarpperGUIDDict.Add(card.UniqueID, new UniqueIDScriptablePack(card, card_dir, CardPath));
                                    else
                                        UnityEngine.Debug.LogWarningFormat("{0} WaitForWarpperGUIDDict Same Key was Add {1}", ModeName, card.UniqueID);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogErrorFormat("{0} Load CardData Error {1}", ModeName, ex.Message);
                        }

                        // Load GameSourceModify
                        try
                        {
                            var modify_dirs = Directory.GetDirectories(dir + @"\GameSourceModify");
                            foreach (var modify_dir in modify_dirs)
                            {
                                var modify_files = Directory.GetFiles(modify_dir);
                                if (modify_files.Length == 1 && modify_files[0].EndsWith(".json"))
                                {
                                    string Guid = Path.GetFileNameWithoutExtension(modify_files[0]);
                                    if (AllGUIDDict.TryGetValue(Guid, out var obj))
                                        WaitForWarpperGameSourceGUIDList.Add(new UniqueIDScriptablePack(obj, modify_dir, modify_files[0]));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogErrorFormat("{0} Load GameSourceModify Error {1}", ModeName, ex.Message);
                        }

                        // Load CharacterPerk
                        try
                        {
                            var card_dirs = Directory.GetDirectories(dir + @"\CharacterPerk");
                            foreach (var card_dir in card_dirs)
                            {
                                string CardName = Path.GetFileName(card_dir); ;
                                string CardPath = card_dir + @"\" + CardName + ".json";
                                if (File.Exists(CardPath))
                                {
                                    CharacterPerk card = CharacterPerk.CreateInstance<CharacterPerk>();
                                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                                    using (StreamReader sr = new StreamReader(CardPath))
                                        JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), card);
                                    card.name = CardName;
                                    card.Init();
                                    AllGUIDDict.Add(card.UniqueID, card);
                                    GameLoad.Instance.DataBase.AllData.Add(card);
                                    if (!WaitForWarpperGUIDDict.ContainsKey(card.UniqueID))
                                        WaitForWarpperGUIDDict.Add(card.UniqueID, new UniqueIDScriptablePack(card, card_dir, CardPath));
                                    else
                                        UnityEngine.Debug.LogWarningFormat("{0} WaitForWarpperGUIDDict Same Key was Add {1}", ModeName, card.UniqueID);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogErrorFormat("{0} Load CharacterPerk Error {1}", ModeName, ex.Message);
                        }

                        // Load GameStat
                        try
                        {
                            var card_dirs = Directory.GetDirectories(dir + @"\GameStat");
                            foreach (var card_dir in card_dirs)
                            {
                                string CardName = Path.GetFileName(card_dir); ;
                                string CardPath = card_dir + @"\" + CardName + ".json";
                                if (File.Exists(CardPath))
                                {
                                    GameStat card = GameStat.CreateInstance<GameStat>();
                                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                                    using (StreamReader sr = new StreamReader(CardPath))
                                        JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), card);
                                    card.name = CardName;
                                    card.Init();
                                    AllGUIDDict.Add(card.UniqueID, card);
                                    GameLoad.Instance.DataBase.AllData.Add(card);
                                    if (!WaitForWarpperGUIDDict.ContainsKey(card.UniqueID))
                                        WaitForWarpperGUIDDict.Add(card.UniqueID, new UniqueIDScriptablePack(card, card_dir, CardPath));
                                    else
                                        UnityEngine.Debug.LogWarningFormat("{0} WaitForWarpperGUIDDict Same Key was Add {1}", ModeName, card.UniqueID);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogErrorFormat("{0} Load GameStat Error {1}", ModeName, ex.Message);
                        }

                        // Load Objective
                        try
                        {
                            var card_dirs = Directory.GetDirectories(dir + @"\Objective");
                            foreach (var card_dir in card_dirs)
                            {
                                string CardName = Path.GetFileName(card_dir); ;
                                string CardPath = card_dir + @"\" + CardName + ".json";
                                if (File.Exists(CardPath))
                                {
                                    Objective card = Objective.CreateInstance<Objective>();
                                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                                    using (StreamReader sr = new StreamReader(CardPath))
                                        JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), card);
                                    card.name = CardName;
                                    card.Init();
                                    AllGUIDDict.Add(card.UniqueID, card);
                                    GameLoad.Instance.DataBase.AllData.Add(card);
                                    if (!WaitForWarpperGUIDDict.ContainsKey(card.UniqueID))
                                        WaitForWarpperGUIDDict.Add(card.UniqueID, new UniqueIDScriptablePack(card, card_dir, CardPath));
                                    else
                                        UnityEngine.Debug.LogWarningFormat("{0} WaitForWarpperGUIDDict Same Key was Add {1}", ModeName, card.UniqueID);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogErrorFormat("{0} Load Objective Error {1}", ModeName, ex.Message);
                        }

                        // Load SelfTriggeredAction
                        try
                        {
                            var card_dirs = Directory.GetDirectories(dir + @"\SelfTriggeredAction");
                            foreach (var card_dir in card_dirs)
                            {
                                string CardName = Path.GetFileName(card_dir); ;
                                string CardPath = card_dir + @"\" + CardName + ".json";
                                if (File.Exists(CardPath))
                                {
                                    SelfTriggeredAction card = SelfTriggeredAction.CreateInstance<SelfTriggeredAction>();
                                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                                    using (StreamReader sr = new StreamReader(CardPath))
                                        JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), card);
                                    card.name = CardName;
                                    card.Init();
                                    AllGUIDDict.Add(card.UniqueID, card);
                                    GameLoad.Instance.DataBase.AllData.Add(card);
                                    if (!WaitForWarpperGUIDDict.ContainsKey(card.UniqueID))
                                        WaitForWarpperGUIDDict.Add(card.UniqueID, new UniqueIDScriptablePack(card, card_dir, CardPath));
                                    else
                                        UnityEngine.Debug.LogWarningFormat("{0} WaitForWarpperGUIDDict Same Key was Add {1}", ModeName, card.UniqueID);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogErrorFormat("{0} Load SelfTriggeredAction Error {1}", ModeName, ex.Message);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.LogError(ex.Message);
            }
        }

        private static void WarpperAllMods()
        {
            DateTime before = DateTime.Now;
            foreach (var item in WaitForWarpperGUIDDict)
            {
                try
                {
                    if (item.Value.obj is CardData)
                    {
                        CardDataWarpper warpper = new CardDataWarpper(item.Value.CardDir);
                        using (StreamReader sr = new StreamReader(item.Value.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.Value.obj as CardData);
                        if ((item.Value.obj as CardData).CardType == CardTypes.Blueprint && warpper.BlueprintCardDataCardTabGroup != "" && warpper.BlueprintCardDataCardTabSubGroup != "")
                            WaitForAddBlueprintCard.Add(new Tuple<string, string, CardData>(warpper.BlueprintCardDataCardTabGroup, warpper.BlueprintCardDataCardTabSubGroup, item.Value.obj as CardData));
                    }
                    else if (item.Value.obj is CharacterPerk)
                    {
                        CharacterPerkWarpper warpper = new CharacterPerkWarpper(item.Value.CardDir);
                        using (StreamReader sr = new StreamReader(item.Value.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.Value.obj as CharacterPerk);
                    }
                    else if (item.Value.obj is GameStat)
                    {
                        GameStatWarpper warpper = new GameStatWarpper(item.Value.CardDir);
                        using (StreamReader sr = new StreamReader(item.Value.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.Value.obj as GameStat);
                        if (warpper.VisibleGameStatStatListTab != "")
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
                    Debug.LogError("WarpperAllMods " + ex.Message);
                }
            }
            DateTime after = DateTime.Now;
            TimeSpan duration = after.Subtract(before);
            Debug.Log("Time taken in Milliseconds: " + (duration.Milliseconds));
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
                        Debug.LogError("LoadLocalization " + ex.Message);
                    }
                }
            }
        }

        private static void AddBlueprintCardData(GraphicsManager instance)
        {
            foreach(var tuple in WaitForAddBlueprintCard)
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
                    Debug.LogError("AddBlueprintCardData " + ex.Message);
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
                catch(Exception ex)
                {
                    Debug.LogError("AddVisibleGameStat " + ex.Message);
                }
            }
        }

        private static void WarpperAllGameSrouces()
        {
            foreach (var item in WaitForWarpperGameSourceGUIDList)
            {
                try
                {
                    if (item.obj is CardData)
                    {
                        CardDataWarpper warpper = new CardDataWarpper(item.CardDir);
                        using (StreamReader sr = new StreamReader(item.CardPath))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                        warpper.WarpperCustomSelf(item.obj as CardData);
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
                    Debug.LogError("WarpperAllGameSrouces " + ex.Message);
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameLoad), "LoadGameData")]
        public static void GameLoadLoadGameDataPostfix()
        {
            try
            {
                LoadGameResource();

                LoadMods();

                LoadLocalization();

                WarpperAllMods();

                WarpperAllGameSrouces();
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);
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
                Debug.LogError(ex.Message);
            }
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(BlueprintModelsScreen), "Awake")]
        //public static void BlueprintModelsScreenAwakePrefix(BlueprintModelsScreen __instance)
        //{
        //    // only init once
        //    AddBlueprint(__instance);
        //}

        [HarmonyPostfix, HarmonyPatch(typeof(GraphicsManager), "Init")]
        public static void GraphicsManagerAwakePostfix(GraphicsManager __instance)
        {
            try
            {
                // only init once
                AddBlueprintCardData(__instance);

                AddVisibleGameStat(__instance);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }
}
