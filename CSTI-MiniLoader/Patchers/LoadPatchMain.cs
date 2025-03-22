using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using CSTI_MiniLoader.LoadUtil;
using CSTI_MiniLoader.LoadUtil.DataFind;
using HarmonyLib;
using MelonLoader;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using Exception = System.Exception;
using Object = UnityEngine.Object;

namespace CSTI_MiniLoader.Patchers;

[HarmonyPatch]
public static class LoadPatchMain
{
    [HarmonyPostfix, HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.LoadLanguage))]
    public static void LocalizationManagerLoadLanguagePostfix()
    {
        try
        {
            LoadLocalization();
        }
        catch (Exception ex)
        {
            MelonLogger.Warning(ex.Message);
        }
    }

    private static void LoadLocalization()
    {
        var currentTexts = LocalizationManager.CurrentTexts;
        if (LocalizationManager.Instance.Languages[LocalizationManager.CurrentLanguage].LanguageName == "简体中文")
            foreach (var pair in WaitForLoadCSVList)
                try
                {
                    if (pair.LocalName.Contains("SimpCn"))
                    {
                        var dictionary = CSVParser.LoadFromString(pair.LocalContent);
                        foreach (var keyValuePair in dictionary)
                            if (!currentTexts.ContainsKey(keyValuePair.Key) && keyValuePair.Value.Count >= 2)
                            {
                                var chLocal = keyValuePair.Value.get_Item(1);
                                if (!string.IsNullOrWhiteSpace(chLocal.Trim()))
                                    currentTexts.Add(keyValuePair.Key, chLocal);
                            }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning("LoadLocalization " + ex.Message);
                }

        if (LocalizationManager.Instance.Languages[LocalizationManager.CurrentLanguage].LanguageName == "English")
            foreach (var pair in WaitForLoadCSVList)
                try
                {
                    if (pair.LocalName.Contains("SimpEn"))
                    {
                        var dictionary = CSVParser.LoadFromString(pair.LocalContent);
                        foreach (var keyValuePair in dictionary)
                            if (!currentTexts.ContainsKey(keyValuePair.Key) && keyValuePair.Value.Count >= 2)
                            {
                                var enLocal = keyValuePair.Value.get_Item(0);
                                if (!string.IsNullOrWhiteSpace(enLocal.Trim()))
                                    currentTexts.Add(keyValuePair.Key, enLocal);
                            }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning("LoadLocalization " + ex.Message);
                }
    }

    public static void Deconstruct<TKey, TVal>(this KeyValuePair<TKey, TVal> pair, out TKey key, out TVal val)
    {
        key = pair.Key;
        val = pair.Value;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GuideManager), "GenerateAllPages")]
    public static void GuideManagerStartPrefix(GuideManager __instance)
    {
        try
        {
            LoadGuideEntry(__instance);

            AddPlayerCharacter(__instance);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning(ex.Message);
        }
    }

    private static void AddPlayerCharacter(GuideManager instance)
    {
        try
        {
            MelonCoroutines.Start(WaiterForContentDisplayer());
        }
        catch (Exception ex)
        {
            MelonLogger.Warning("AddPlayerCharacter" + ex.Message);
        }
    }


    public static bool OnceWarp;

    private static IEnumerator WaiterForContentDisplayer()
    {
        var done = false;
        while (true)
        {
            Object[] objs;
            try
            {
                objs = Resources.FindObjectsOfTypeAll(Il2CppType.Of<ContentDisplayer>()).ToArray();
            }
            catch (Exception e)
            {
                try
                {
                    objs = [Resources.Load("Assets/JournalTourist").Cast<Object>()];
                    if (objs[0] == null)
                    {
                        objs = [];
                    }
                }
                catch (Exception exception)
                {
                    MelonLogger.Error(exception);
                    objs = [];
                }
            }


            foreach (var o in objs)
            {
                var obj = o.Cast<ContentDisplayer>();
                if (obj.gameObject.name != "JournalTourist") continue;
                ContentDisplayer? displayer = null;
                GameObject? clone = null;
                try
                {
                    clone = Object.Instantiate(obj.gameObject);
                    displayer = clone.GetComponent<ContentDisplayer>();
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning("FXMask Warning " + ex.Message);
                }

                if (displayer == null)
                    break;
                if (clone != null)
                {
                    clone.name = "JournalDefaultSample";
                    clone.hideFlags = HideFlags.HideAndDontSave;
                    CustomGameObjectListDict.Add(clone.name, clone);
                    CustomContentDisplayerDict.Add(clone.name, displayer);
                    done = true;
                }

                break;
            }

            if (done) break;

            yield return new WaitForSeconds(0.5f);
        }

        var displayers = Resources.FindObjectsOfTypeAll(Il2CppType.Of<ContentDisplayer>());
        foreach (var displayer in displayers)
            try
            {
                if (!CustomContentDisplayerDict.ContainsKey(displayer.name) &&
                    displayer is ContentDisplayer contentDisplayer)
                    CustomContentDisplayerDict.Add(displayer.name, contentDisplayer);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("CustomContentDisplayerDict Warning " + ex.Message);
            }

        while (!OnceWarp) yield return null;

        while (WaitForAddDefaultContentPage.Count > 0)
        {
            var item = WaitForAddDefaultContentPage.Pop();
            try
            {
                if (item.Obj != null && CustomGameObjectListDict.ContainsKey(item.Obj.name))
                    continue;
                if (CustomGameObjectListDict.TryGetValue("JournalDefaultSample", out var sample))
                {
                    GameObject? clone = null;
                    ContentDisplayer? displayer = null;
                    try
                    {
                        clone = Object.Instantiate(sample);
                        displayer = clone.GetComponent(Il2CppType.Of<ContentDisplayer>()) as ContentDisplayer;
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning("FXMask Warning " + ex.Message);
                    }

                    if (displayer == null) continue;

                    var modPage = item.Obj as ContentPage;
                    if (modPage == null) continue;

                    var tDisplayer = Trav.Create(displayer);
                    var pages = tDisplayer.Field("ExplicitPageContent").GetValue<List<ContentPage>>();
                    pages.Clear();
                    pages.Add(modPage);
                    tDisplayer.Field("DefaultPage").SetValue(modPage);

                    if (item.Obj != null)
                    {
                        var nameParts = item.Obj.name.Split(['_']);
                        if (nameParts.Length > 2 && clone != null)
                        {
                            clone.name = nameParts[0] + "_" + nameParts[1];
                            clone.hideFlags = HideFlags.HideAndDontSave;
                            CustomGameObjectListDict.Add(clone.name, clone);
                            CustomContentDisplayerDict.Add(clone.name, displayer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("WaiterForContentDisplayer WaitForAddDefaultContentPage " + ex.Message);
            }
        }

        while (WaitForAddMainContentPage.Count > 0)
        {
            var item = WaitForAddMainContentPage.Pop();
            try
            {
                if (item.Obj != null)
                {
                    var nameParts = item.Obj.name.Split(['_']);
                    if (nameParts.Length > 2 && CustomContentDisplayerDict.TryGetValue(
                            nameParts[0] + "_" + nameParts[1],
                            out var displayer))
                    {
                        var pages = Trav.Create(displayer).Field("ExplicitPageContent").GetValue<List<ContentPage>>();
                        pages?.Add((ContentPage)item.Obj);
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("WaiterForContentDisplayer WaitForAddMainContentPage " + ex.Message);
            }
        }

        while (WaitForAddJournalPlayerCharacter.Count > 0)
        {
            var item = WaitForAddJournalPlayerCharacter.Pop();
            try
            {
                if (item.Obj is not PlayerCharacter character)
                    continue;

                var json = item.CardData;
                if (json != null && json.ContainsKey("PlayerCharacterJournalName") &&
                    json["PlayerCharacterJournalName"].IsString &&
                    !string.IsNullOrWhiteSpace(json["PlayerCharacterJournalName"].ToString()))
                    if (CustomContentDisplayerDict.TryGetValue(json["PlayerCharacterJournalName"].ToString(),
                            out var displayer))
                        character.Journal = displayer;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("WaiterForContentDisplayer PlayerCharacterJournalName " + ex.Message);
            }
        }
    }

    private static void LoadGuideEntry(GuideManager instance)
    {
        try
        {
            // GuideManager.AllEntries  -- 被内联
            foreach (var entry in WaitForAddGuideEntry) instance.AllEntries.Add(entry);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning("LoadGuideEntry" + ex.Message);
        }
    }

    private static bool _inited;
    public static bool CanInit;


    [HarmonyPrefix, HarmonyPatch(typeof(UniqueIDScriptable), nameof(UniqueIDScriptable.Init))]
    public static void RegObjPatch(UniqueIDScriptable __instance)
    {
        RegObj(__instance.UniqueID, __instance, __instance.GetType());
        foreach (var o in __instance.Find())
        {
            if (o == null || o.Equals(null)) continue;
            if (o.Cast<UniqueIDScriptable>() is { } uniqueIDScriptable)
            {
                RegObj(uniqueIDScriptable.UniqueID, uniqueIDScriptable, uniqueIDScriptable.GetType());
            }
            else
            {
                RegObj(o.name, o, o.GetType());
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameLoad), nameof(GameLoad.Update))]
    public static void LoadAndInit()
    {
        if (_inited) return;
        if (!CanInit) return;
        _inited = true;
        MelonLogger.Warning("Begin to miniLoader");
        AllItemDictionary[typeof(Sprite)] = new Dictionary<string, object>();
        AllItemDictionary[typeof(AudioClip)] = new Dictionary<string, object>();
        MelonLogger.Warning("Preset AllItemDictionary");
        try
        {
            MelonLogger.Msg("Try LoadGameResource");
            LoadResources.LoadGameResource();
            MelonLogger.Msg("Try LoadEditorScriptableObject");
            LoadResources.LoadEditorScriptableObject();
            MelonLogger.Msg("Try WarpperAllEditorMods");
            LoadResources.WarpperAllEditorMods();
            MelonLogger.Msg("Try WarpperAllEditorGameSrouces");
            LoadResources.WarpperAllEditorGameSrouces();
            MelonLogger.Msg("Try MatchAndWarpperAllEditorGameSrouce");
            LoadResources.MatchAndWarpperAllEditorGameSrouce();
            MelonLogger.Msg("Try AddPerkGroup");
            AddPerkGroup();
            foreach (var (id, uniqueIDScriptable) in AllGUIDDict)
            {
                uniqueIDScriptable.Init();
            }
        }
        catch (Exception e)
        {
            MelonLogger.Error(e);
        }

        MelonLogger.Warning("miniLoader End");
    }

    private static void AddPerkGroup()
    {
        foreach (var tuple in WaitForAddPerkGroup)
            try
            {
                if (ItemDictionary(typeof(PerkGroup)).TryGetValue(tuple.Item1, out var group))
                {
                    var obj = group as PerkGroup;
                    if (obj != null)
                    {
                        obj.PerksList = obj.PerksList.AddItem(tuple.Item2).ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("AddPerkGroup " + ex.Message);
            }
    }

    private static bool _initFlag;

    [HarmonyPostfix, HarmonyPatch(typeof(GraphicsManager), nameof(GraphicsManager.Init))]
    public static void GraphicsManagerInitPostfix(GraphicsManager __instance)
    {
        try
        {
            AddCardTabGroup(__instance);

            AddBlueprintCardData(__instance);

            AddVisibleGameStat(__instance);

            if (!_initFlag)
            {
                AddCardTabGroupOnce(__instance);

                CustomGameObjectFixed();

                AddCardFilterGroupOnce();
                _initFlag = true;
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Warning(ex.Message);
        }
    }

    private static void AddCardFilterGroupOnce()
    {
        if (!GraphicsManager.Instance) return;
        var cardFilterGroupDict = new Dictionary<string, CardFilterGroup>();


        foreach (var ele in GraphicsManager.Instance.CurrentFilterTags)
        {
            if (ele != null)
                cardFilterGroupDict.Add(ele.name, ele);
        }

        foreach (var item in WaitForAddCardFilterGroupCard)
            if (cardFilterGroupDict.TryGetValue(item.Item1, out var filter))
                filter.IncludedCards.Add(item.Item2);
    }

    private static void CustomGameObjectFixed()
    {
        foreach (var item in CustomGameObjectListDict)
            try
            {
                var transform = item.Value.transform.Find("Shadow/GuideFrame/GuideContentPage/Content/Horizontal");
                if (transform != null)
                    for (var i = 0; i < transform.childCount; i++)
                        Object.Destroy(transform.GetChild(i).gameObject);

                transform = item.Value.transform.Find("Shadow/GuideFrame");
                var fx = transform.gameObject.GetComponent<FXMask>();
                if (fx != null) fx.enabled = true;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("CustomGameObjectFixed " + ex.Message);
            }
    }

    private static void AddCardTabGroupOnce(GraphicsManager instance)
    {
        foreach (var item in WaitForAddCardTabGroup)
            try
            {
                if (item.Obj is not CardTabGroup itemObj)
                    continue;

                itemObj.FillSortingList();

                if (!itemObj.name.StartsWith("Tab_"))
                    continue;

                if (itemObj.SubGroups.Count == 0)
                {
                    var json = item.CardData;
                    if (json != null && json.ContainsKey("BlueprintCardDataCardTabGroup") &&
                        json["BlueprintCardDataCardTabGroup"].IsString && !string.IsNullOrWhiteSpace(
                            json["BlueprintCardDataCardTabGroup"].ToString()))
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
                MelonLogger.Warning("AddCustomCardTabGroup " + ex.Message);
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
                var statList = Trav.Create(instance.AllStatsList).Field("Tabs").GetValue<StatListTab[]>();
                foreach (var list in statList)
                    if (list.name == tuple.Item1)
                    {
                        list.ContainedStats.Add(tuple.Item2);
                        break;
                    }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("AddVisibleGameStat " + ex.Message);
            }
    }

    private static void AddCardTabGroup(GraphicsManager instance)
    {
        foreach (var item in WaitForAddCardTabGroup)
            try
            {
                if (item.Obj is not CardTabGroup tabGroup)
                    continue;

                tabGroup.FillSortingList();

                if (!tabGroup.name.StartsWith("Tab_"))
                    continue;

                if (tabGroup.SubGroups.Count != 0)
                {
                    instance.BlueprintModelsPopup.BlueprintTabs =
                        instance.BlueprintModelsPopup.BlueprintTabs.AddItem(tabGroup).ToArray();
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("AddCardTabGroup " + ex.Message);
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
                        foreach (var subGroup in group.SubGroups)
                            if (subGroup.name == tuple.Item2)
                            {
                                subGroup.IncludedCards.Add(tuple.Item3);
                                break;
                            }

                        break;
                    }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("AddBlueprintCardData " + ex.Message);
            }
    }
}