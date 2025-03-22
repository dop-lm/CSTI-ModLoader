using System;
using System.Collections.Generic;
using System.IO;
using CSTI_MiniLoader.LoadUtil;
using CSTI_MiniLoader.Patchers;
using MelonLoader;
using UnityEngine;

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
        ItemDictionary(type).set_Item(id, o);
        if (type != null && type.IsSubclassOf(typeof(UniqueIDScriptable)))
        {
            AllGUIDDict.set_Item(id, (UniqueIDScriptable)o);
        }

        if (type != null && type.IsSubclassOf(typeof(ScriptableObject)))
        {
            AllScriptableObjectDict.set_Item(id, (ScriptableObject)o);
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
        AllItemDictionary.set_Item(type, objects);
        return objects;
    }

    public override void OnInitializeMelon()
    {
        HarmonyIns.PatchAll(typeof(LoadPatchMain));
        MelonLogger.Msg($"ModPath:{Path.GetDirectoryName(MelonAssembly.Location)}");
    }
}