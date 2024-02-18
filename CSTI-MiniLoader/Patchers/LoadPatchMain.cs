using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CSTI_MiniLoader.LoadUtil;
using HarmonyLib;
using MelonLoader;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CSTI_MiniLoader.Patchers;

[SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
public static class LoadPatchMain
{
    public static void Deconstruct<TKey, TVal>(this KeyValuePair<TKey, TVal> pair, out TKey key, out TVal val)
    {
        key = pair.Key;
        val = pair.Value;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GuideManager), nameof(GuideManager.Start))]
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

    private static void AddPlayerCharacter(GuideManager instance)
    {
        try
        {
            MelonCoroutines.Start(WaiterForContentDisplayer());
        }
        catch (Exception ex)
        {
            Debug.LogWarning("AddPlayerCharacter" + ex.Message);
        }
    }


    public static bool OnceWarp;

    private static IEnumerator WaiterForContentDisplayer()
    {
        var done = false;
        while (true)
        {
            var objs = Resources.FindObjectsOfTypeAll(Il2CppType.Of<ContentDisplayer>());

            foreach (var o in objs)
            {
                var obj = (ContentDisplayer)o;
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
                    Debug.LogWarning("FXMask Warning " + ex.Message);
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
                Debug.LogWarning("CustomContentDisplayerDict Warning " + ex.Message);
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
                        Debug.LogWarning("FXMask Warning " + ex.Message);
                    }

                    if (displayer == null) continue;

                    var modPage = item.Obj as ContentPage;
                    if (modPage == null) continue;

                    var tDisplayer = Traverse.Create(displayer);
                    var pages = tDisplayer.Field<List<ContentPage>>("ExplicitPageContent").Value;
                    pages.Clear();
                    pages.Add(modPage);
                    tDisplayer.Field<ContentPage>("DefaultPage").Value = modPage;

                    if (item.Obj != null)
                    {
                        var nameParts = item.Obj.name.Split('_');
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
                Debug.LogWarning("WaiterForContentDisplayer WaitForAddDefaultContentPage " + ex.Message);
            }
        }

        while (WaitForAddMainContentPage.Count > 0)
        {
            var item = WaitForAddMainContentPage.Pop();
            try
            {
                if (item.Obj != null)
                {
                    var nameParts = item.Obj.name.Split('_');
                    if (nameParts.Length > 2 && CustomContentDisplayerDict.TryGetValue(
                            nameParts[0] + "_" + nameParts[1],
                            out var displayer))
                    {
                        var pages = displayer.ExplicitPageContent;
                        pages?.Add((ContentPage)item.Obj);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("WaiterForContentDisplayer WaitForAddMainContentPage " + ex.Message);
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
                Debug.LogWarning("WaiterForContentDisplayer PlayerCharacterJournalName " + ex.Message);
            }
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

    [HarmonyPrefix, HarmonyPatch(typeof(UniqueIDScriptable), nameof(UniqueIDScriptable.ClearDict))]
    public static void UniqueIDScriptableClearDictPrefix()
    {
        AllItemDictionary[typeof(Sprite)] = new Dictionary<string, object>();
        AllItemDictionary[typeof(AudioClip)] = new Dictionary<string, object>();
        try
        {
            LoadResources.LoadGameResource();
            LoadArchMod.LoadAllArchMod();
            LoadResources.LoadEditorScriptableObject();
            LoadResources.WarpperAllEditorMods();
            LoadResources.WarpperAllEditorGameSrouces();
            LoadResources.MatchAndWarpperAllEditorGameSrouce();
        }
        catch (Exception e)
        {
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
            Debug.LogWarning(ex.Message);
        }
    }

    private static void AddCardFilterGroupOnce()
    {
        var cardFilterGroupDict = new Dictionary<string, CardFilterGroup>();

        foreach (var ele in Resources.FindObjectsOfTypeAll(Il2CppType.Of<CardFilterGroup>()))
        {
            if (ele is CardFilterGroup cardFilterGroup)
                cardFilterGroupDict.Add(ele.name, cardFilterGroup);
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
                var fx = transform.gameObject.GetComponent(Il2CppType.Of<FXMask>()) as FXMask;
                if (fx != null) fx.enabled = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("CustomGameObjectFixed " + ex.Message);
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
                Debug.LogWarning("AddCustomCardTabGroup " + ex.Message);
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
                var statList = instance.AllStatsList.Tabs;
                foreach (var list in statList)
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
                    Il2CppArrayBase<CardTabGroup> il2CppReferenceArray = instance.BlueprintModelsPopup.BlueprintTabs;
                    Array.Resize<>(ref il2CppReferenceArray,
                        instance.BlueprintModelsPopup.BlueprintTabs.Length + 1);
                    instance.BlueprintModelsPopup.BlueprintTabs =
                        (Il2CppReferenceArray<CardTabGroup>)il2CppReferenceArray;
                    instance.BlueprintModelsPopup.BlueprintTabs[^1] = tabGroup;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("AddCardTabGroup " + ex.Message);
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
                Debug.LogWarning("AddBlueprintCardData " + ex.Message);
            }
    }
}