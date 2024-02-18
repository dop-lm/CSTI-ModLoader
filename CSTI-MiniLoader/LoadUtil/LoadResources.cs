using System;
using System.Collections.Generic;
using System.Linq;
using CSTI_MiniLoader.LoadUtil.DataFind;
using CSTI_MiniLoader.Patchers;
using CSTI_MiniLoader.WarpperClassGen;
using MelonLoader;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CSTI_MiniLoader.LoadUtil;

public static class LoadResources
{
    public static T Pop<T>(this List<T> list)
    {
        if (list.Count == 0) return default;
        var result = list[^1];
        list.RemoveAt(list.Count - 1);
        return result;
    }

    public static void LoadEditorScriptableObject()
    {
        while (WaitForWarpperEditorNoGuidList.Count > 0)
        {
            var item = WaitForWarpperEditorNoGuidList.Pop();
            try
            {
                var json = item.CardData;
                if (json == null) continue;
                WarpFunc.JsonCommonWarpper(item.Obj, json);
                if (item.Obj is CardTabGroup && item.Obj.name.StartsWith("Tab_"))
                    WaitForAddCardTabGroup.Add(new ScriptableObjectPack(item.Obj, "", "", "", item.CardData));

                if (item.Obj is ContentPage)
                {
                    if (item.Obj.name.EndsWith("Default"))
                        WaitForAddDefaultContentPage.Add(new ScriptableObjectPack(item.Obj, "", "", "",
                            item.CardData));
                    else if (item.Obj.name.EndsWith("Main"))
                        WaitForAddMainContentPage.Add(new ScriptableObjectPack(item.Obj, "", "", "",
                            item.CardData));
                }

                if (item.Obj is GuideEntry entry) WaitForAddGuideEntry.Add(entry);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("LoadEditorScriptableObject " + ex.Message);
            }
        }
    }

    public static IEnumerable<Object> WithGameDataFinder(this IEnumerable<Object> enumerable)
    {
        foreach (var o in enumerable)
        {
            yield return o;
        }

        foreach (var data in GameLoad.Instance.DataBase.AllData)
        {
            yield return data;
            foreach (var o in data.Find())
            {
                yield return o;
            }
        }
    }

    public static void LoadGameResource()
    {
        try
        {
            foreach (var ele in Object.FindObjectsOfType(Il2CppType.Of<ScriptableObject>()).WithGameDataFinder())
            {
                if (ele is not UniqueIDScriptable)
                {
                    RegObj(ele.name, ele, ele.GetType());
                }
                else if (ele is UniqueIDScriptable idScriptable)
                {
                    RegObj(idScriptable.UniqueID, idScriptable, idScriptable.GetType());
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error("LoadGameResource Error " + ex.Message);
        }
    }


    public static void WarpperAllEditorMods()
    {
        // var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        var keys = WaitForWarpperEditorGuidDict.Keys.ToList();
        foreach (var key in keys)
        {
            try
            {
                var processingScriptableObjectPack = WaitForWarpperEditorGuidDict[key];
                WaitForWarpperEditorGuidDict.Remove(key);

                var json = processingScriptableObjectPack.CardData;
                if (json == null) continue;
                WarpFunc.JsonCommonWarpper(processingScriptableObjectPack.Obj, json);
                if (processingScriptableObjectPack.Obj is CardData cardData)
                {
                    if (cardData.CardType == CardTypes.Blueprint &&
                        json.ContainsKey("BlueprintCardDataCardTabGroup") &&
                        json["BlueprintCardDataCardTabGroup"].IsString && !string.IsNullOrWhiteSpace(
                            json["BlueprintCardDataCardTabGroup"].ToString()) &&
                        json.ContainsKey("BlueprintCardDataCardTabSubGroup") &&
                        json["BlueprintCardDataCardTabSubGroup"].IsString &&
                        !string.IsNullOrWhiteSpace(json["BlueprintCardDataCardTabSubGroup"].ToString()))
                        WaitForAddBlueprintCard.Add(new Tuple<string, string, CardData>(
                            json["BlueprintCardDataCardTabGroup"].ToString(),
                            json["BlueprintCardDataCardTabSubGroup"].ToString(), cardData));

                    if (json.ContainsKey("ItemCardDataCardTabGpGroup") &&
                        json["ItemCardDataCardTabGpGroup"].IsArray)
                        for (var i = 0; i < json["ItemCardDataCardTabGpGroup"].Count; i++)
                            if (json["ItemCardDataCardTabGpGroup"][i].IsString &&
                                ItemDictionary(typeof(CardTabGroup)).TryGetValue(
                                    json["ItemCardDataCardTabGpGroup"][i].ToString(),
                                    out var tabGroup))
                                (tabGroup as CardTabGroup)!.IncludedCards.Add(cardData);

                    if (json.ContainsKey("CardDataCardFilterGroup") && json["CardDataCardFilterGroup"].IsArray)
                        for (var i = 0; i < json["CardDataCardFilterGroup"].Count; i++)
                            if (json["CardDataCardFilterGroup"][i].IsString && !string.IsNullOrWhiteSpace(
                                    json["CardDataCardFilterGroup"][i].ToString()))
                                WaitForAddCardFilterGroupCard.Add(new Tuple<string, CardData>(
                                    json["CardDataCardFilterGroup"][i].ToString(), cardData));

                    cardData.FillDropsList();
                    // var FillDropsList = typeof(CardData).GetMethod("FillDropsList", bindingFlags);
                    // if (FillDropsList != null)
                    // {
                    //     FillDropsList.Invoke(item.Value.obj, null);
                    // }
                }
                else if (processingScriptableObjectPack.Obj is CharacterPerk perk)
                {
                    if (json.ContainsKey("CharacterPerkPerkGroup") && json["CharacterPerkPerkGroup"].IsString &&
                        !string.IsNullOrWhiteSpace(json["CharacterPerkPerkGroup"].ToString()))
                        WaitForAddPerkGroup.Add(new Tuple<string, CharacterPerk>(
                            json["CharacterPerkPerkGroup"].ToString(), perk));
                }
                else if (processingScriptableObjectPack.Obj is GameStat stat)
                {
                    if (json.ContainsKey("VisibleGameStatStatListTab") &&
                        json["VisibleGameStatStatListTab"].IsString &&
                        !string.IsNullOrWhiteSpace(json["VisibleGameStatStatListTab"].ToString()))
                        WaitForAddVisibleGameStat.Add(new Tuple<string, GameStat>(
                            json["VisibleGameStatStatListTab"].ToString(), stat));
                }
                else if (processingScriptableObjectPack.Obj is PlayerCharacter character)
                {
                    foreach (var pair in ItemDictionary(typeof(Gamemode)))
                    {
                        var mode = pair.Value as Gamemode;
                        var il2CppReferenceArray = (Il2CppArrayBase<PlayerCharacter>)mode.PlayableCharacters;
                        Il2CppSystem.Array.Resize(ref il2CppReferenceArray, mode.PlayableCharacters.Length + 1);
                        mode.PlayableCharacters = (Il2CppReferenceArray<PlayerCharacter>)il2CppReferenceArray;
                        mode.PlayableCharacters[^1] = character;
                    }

                    WaitForAddJournalPlayerCharacter.Add(new ScriptableObjectPack(character, "", "", "",
                        processingScriptableObjectPack.CardData));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("WarpperAllEditorMods " + ex.Message);
            }
        }

        LoadPatchMain.OnceWarp = true;
    }

    public static void MatchAndWarpperAllEditorGameSrouce()
    {
        foreach (var item in AllGUIDDict.Values)
        {
            try
            {
                if (item is CardData cardData)
                {
                    foreach (var tag in cardData.CardTags)
                    {
                        if (!AllCardTagGuidCardDataDict.ContainsKey(tag.name))
                            AllCardTagGuidCardDataDict.Add(tag.name, new Dictionary<string, CardData>());

                        if (AllCardTagGuidCardDataDict.TryGetValue(tag.name, out var dict))
                            dict.Add(cardData.UniqueID, cardData);
                    }
                }
            }
            catch
            {
                //Debug.LogWarning("MatchAndWarpperAllEditorGameSrouce Match " + ex.Message);
            }
        }

        while (WaitForMatchAndWarpperEditorGameSourceList.Count > 0)
        {
            var item = WaitForMatchAndWarpperEditorGameSourceList.Pop();
            try
            {
                if (item.CardData == null)
                    continue;
                var json = item.CardData;

                if (json.ContainsKey("MatchTagWarpData") && json["MatchTagWarpData"].IsArray &&
                    json["MatchTagWarpData"].Count > 0)
                {
                    if (!AllCardTagGuidCardDataDict.TryGetValue(json["MatchTagWarpData"][0].ToString(),
                            out var dict))
                        continue;
                    var MatchList = dict.Keys.ToList();

                    for (var i = 1; i < json["MatchTagWarpData"].Count; i++)
                    {
                        if (AllCardTagGuidCardDataDict.TryGetValue(json["MatchTagWarpData"][i].ToString(),
                                out var next_dict))
                            MatchList = MatchList.Intersect(next_dict.Keys).ToList();
                    }

                    foreach (var match in MatchList)
                    {
                        if (AllGUIDDict.TryGetValue(match, out var card))
                        {
                            if (card is CardData cardData)
                            {
                                if (json.ContainsKey("MatchTypeWarpData") && json["MatchTypeWarpData"].IsString)
                                    if (cardData.CardType.ToString() !=
                                        json["MatchTypeWarpData"].ToString())
                                        continue;
                                WarpFunc.JsonCommonWarpper(card, json);
                                cardData.FillDropsList();
                                // var FillDropsList = typeof(CardData).GetMethod("FillDropsList", bindingFlags);
                                // if (FillDropsList != null)
                                //     FillDropsList.Invoke(card, null);
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


    public static void WarpperAllEditorGameSrouces()
    {
        // var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //foreach (var item in WaitForWarpperEditorGameSourceGUIDList)
        while (WaitForWarpperEditorGameSourceGUIDList.Count > 0)
        {
            var item = WaitForWarpperEditorGameSourceGUIDList.Pop();
            try
            {
                if (item.Obj == null)
                {
                    if (AllGUIDDict.TryGetValue(item.CardDirOrGuid, out var obj))
                        item.Obj = obj;
                    else
                        continue;
                }

                var processingScriptableObjectPack = item;

                if (item.CardData != null)
                {
                    var json = item.CardData;
                    if (json.ContainsKey("MatchTagWarpData") && json["MatchTagWarpData"].IsArray &&
                        json["MatchTagWarpData"].Count > 0)
                    {
                        WaitForMatchAndWarpperEditorGameSourceList.Add(item);
                        continue;
                    }

                    if (json.ContainsKey("ModLoaderSpecialOverwrite") && json["ModLoaderSpecialOverwrite"].IsBoolean &&
                        (bool)json["ModLoaderSpecialOverwrite"])
                    {
                        JsonUtility.FromJsonOverwrite(item.CardData.ToJson(), item.Obj);
                    }

                    WarpFunc.JsonCommonWarpper(item.Obj, json);
                }

                if (item.Obj is CardData cardData)
                {
                    cardData.FillDropsList();
                    // var FillDropsList = typeof(CardData).GetMethod("FillDropsList", bindingFlags);
                    // if (FillDropsList != null)
                    // {
                    //     FillDropsList.Invoke(item.Obj, null);
                    // }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("WarpperAllEditorGameSrouces " + ex.Message);
            }
        }
    }
}