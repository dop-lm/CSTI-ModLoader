using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using LitJson;
using ModLoader.ExportUtil;
using UnityEngine;

namespace ModLoader.LoaderUtil;

public static class DoWarpperLoader
{
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

        foreach (var item in WaitForMatchAndWarpperEditorGameSourceList)
        {
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
                                WarpperFunction.JsonCommonWarpper(card, json);
                                Traverse.Create(cardData).Method("FillDropsList")?.GetValue();
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
        for (var i = 0; i < WaitForWarpperEditorGameSourceGUIDList.Count; i++)
        {
            var item = WaitForWarpperEditorGameSourceGUIDList[i];
            try
            {
                if (item.obj == null)
                {
                    if (AllGUIDDict.TryGetValue(item.CardDirOrGuid, out var obj))
                        item.obj = obj;
                    else
                        continue;
                }

                ProcessingScriptableObjectPack = item;

                if (item.CardData!=null)
                {
                    var json = item.CardData;
                    if (json.ContainsKey("MatchTagWarpData") && json["MatchTagWarpData"].IsArray &&
                        json["MatchTagWarpData"].Count > 0)
                    {
                        WaitForMatchAndWarpperEditorGameSourceList.Add(item);
                        continue;
                    }

                    if (json.ContainsKey("ModLoaderSpecialOverwrite"))
                        if (json["ModLoaderSpecialOverwrite"].IsBoolean && (bool) json["ModLoaderSpecialOverwrite"])
                            JsonUtility.FromJsonOverwrite(item.CardData.ToJson(), item.obj);
                    WarpperFunction.JsonCommonWarpper(item.obj, json);
                }

                if (item.obj is CardData cardData)
                {
                    Traverse.Create(cardData).Method("FillDropsList")?.GetValue();
                    // var FillDropsList = typeof(CardData).GetMethod("FillDropsList", bindingFlags);
                    // if (FillDropsList != null)
                    // {
                    //     FillDropsList.Invoke(item.obj, null);
                    // }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("WarpperAllEditorGameSrouces " + ex.Message);
            }
        }
    }

        
    public static void WarpperAllEditorMods()
    {
        if (!_onceWarp.DoOnce())
        {
            return;
        }

        // var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        foreach (var item in WaitForWarpperEditorGuidDict)
        {
            try
            {
                ProcessingScriptableObjectPack = item.Value;

                var json = item.Value.CardData;
                if(json==null)continue;
                WarpperFunction.JsonCommonWarpper(item.Value.obj, json);
                if (item.Value.obj is CardData cardData)
                {
                    if (cardData.CardType == CardTypes.Blueprint &&
                        json.ContainsKey("BlueprintCardDataCardTabGroup") &&
                        json["BlueprintCardDataCardTabGroup"].IsString && !json["BlueprintCardDataCardTabGroup"]
                            .ToString().IsNullOrWhiteSpace() &&
                        json.ContainsKey("BlueprintCardDataCardTabSubGroup") &&
                        json["BlueprintCardDataCardTabSubGroup"].IsString &&
                        !json["BlueprintCardDataCardTabSubGroup"].ToString().IsNullOrWhiteSpace())
                        WaitForAddBlueprintCard.Add(new Tuple<string, string, CardData>(
                            json["BlueprintCardDataCardTabGroup"].ToString(),
                            json["BlueprintCardDataCardTabSubGroup"].ToString(), cardData));

                    if (json.ContainsKey("ItemCardDataCardTabGpGroup") &&
                        json["ItemCardDataCardTabGpGroup"].IsArray &&
                        AllScriptableObjectWithoutGuidTypeDict.TryGetValue(typeof(CardTabGroup), out var dict))
                        for (var i = 0; i < json["ItemCardDataCardTabGpGroup"].Count; i++)
                            if (json["ItemCardDataCardTabGpGroup"][i].IsString &&
                                dict.TryGetValue(json["ItemCardDataCardTabGpGroup"][i].ToString(),
                                    out var tab_group))
                                (tab_group as CardTabGroup)!.IncludedCards.Add(cardData);

                    if (json.ContainsKey("CardDataCardFilterGroup") && json["CardDataCardFilterGroup"].IsArray)
                        for (var i = 0; i < json["CardDataCardFilterGroup"].Count; i++)
                            if (json["CardDataCardFilterGroup"][i].IsString && !json["CardDataCardFilterGroup"][i]
                                    .ToString().IsNullOrWhiteSpace())
                                WaitForAddCardFilterGroupCard.Add(new Tuple<string, CardData>(
                                    json["CardDataCardFilterGroup"][i].ToString(), cardData));

                    Traverse.Create(cardData).Method("FillDropsList")?.GetValue();
                    // var FillDropsList = typeof(CardData).GetMethod("FillDropsList", bindingFlags);
                    // if (FillDropsList != null)
                    // {
                    //     FillDropsList.Invoke(item.Value.obj, null);
                    // }
                }
                else if (item.Value.obj is CharacterPerk)
                {
                    if (json.ContainsKey("CharacterPerkPerkGroup") && json["CharacterPerkPerkGroup"].IsString &&
                        !json["CharacterPerkPerkGroup"].ToString().IsNullOrWhiteSpace())
                        WaitForAddPerkGroup.Add(new Tuple<string, CharacterPerk>(
                            json["CharacterPerkPerkGroup"].ToString(), item.Value.obj as CharacterPerk));
                }
                else if (item.Value.obj is GameStat)
                {
                    if (json.ContainsKey("VisibleGameStatStatListTab") &&
                        json["VisibleGameStatStatListTab"].IsString &&
                        !json["VisibleGameStatStatListTab"].ToString().IsNullOrWhiteSpace())
                        WaitForAddVisibleGameStat.Add(new Tuple<string, GameStat>(
                            json["VisibleGameStatStatListTab"].ToString(), item.Value.obj as GameStat));
                }
                else if (item.Value.obj is PlayerCharacter)
                {
                    if (AllGUIDTypeDict.TryGetValue(typeof(Gamemode), out var dict))
                    {
                        foreach (var pair in dict)
                        {
                            var mode = pair.Value as Gamemode;
                            Array.Resize(ref mode.PlayableCharacters,
                                mode.PlayableCharacters.Length + 1);
                            mode.PlayableCharacters[mode.PlayableCharacters.Length - 1] =
                                item.Value.obj as PlayerCharacter;
                        }
                    }

                    WaitForAddJournalPlayerCharacter.Add(new ScriptableObjectPack(item.Value.obj, "", "", "",
                        item.Value.CardData));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("WarpperAllEditorMods " + ex.Message);
            }
        }

        _onceWarp.SetDone();
    }
}