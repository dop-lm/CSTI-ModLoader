using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace ModLoader.LoaderUtil;

public static class DoWarpperLoader
{
    public static PerkTabGroup? PerkTabGroup_All;

    public static T Pop<T>(this List<T> list)
    {
        if (list.Count == 0) return default;
        var result = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return result;
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
        while (WaitForWarpperEditorGameSourceGUIDList.Count > 0)
        {
            var item = WaitForWarpperEditorGameSourceGUIDList.Pop();
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
                        // if (item.obj is IModLoaderJsonObj modLoaderJsonObj)
                        // {
                        //     modLoaderJsonObj.CreateByJson(item.CardData.ToJson());
                        // }
                        // else
                        {
                            JsonUtility.FromJsonOverwrite(item.CardData.ToJson(), item.obj);
                        }
                    }

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
        var keys = WaitForWarpperEditorGuidDict.Keys.ToList();
        foreach (var key in keys)
        {
            try
            {
                ProcessingScriptableObjectPack = WaitForWarpperEditorGuidDict[key];
                WaitForWarpperEditorGuidDict.Remove(key);

                var json = ProcessingScriptableObjectPack.CardData;
                if (json == null) continue;
                WarpperFunction.JsonCommonWarpper(ProcessingScriptableObjectPack.obj, json);
                if (ProcessingScriptableObjectPack.obj is CardData cardData)
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
                                    out var tabGroup))
                                (tabGroup as CardTabGroup)!.IncludedCards.Add(cardData);

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

                    if (cardData.OldDefaultEnvCards != null)
                    {
                        if (cardData.DefaultEnvCardDrops == null || cardData.DefaultEnvCardDrops.Length == 0)
                        {
                            cardData.DefaultEnvCardDrops =
                                cardData.OldDefaultEnvCards.Select(data => new CardDrop(data)).ToArray();
                        }
                    }

                    if (cardData.EffectsToInventoryContent is { Length: > 0 })
                    {
                        for (int i = 0; i < cardData.EffectsToInventoryContent.Length; i++)
                        {
                            var pe = cardData.EffectsToInventoryContent[i];
                            pe.DroppedCards ??= Array.Empty<CardsDropCollection>();
                            cardData.EffectsToInventoryContent[i] = pe;
                        }
                    }

                    if (cardData.PassiveEffects is { Length: > 0 })
                    {
                        for (int i = 0; i < cardData.PassiveEffects.Length; i++)
                        {
                            var pe = cardData.PassiveEffects[i];
                            pe.DroppedCards ??= Array.Empty<CardsDropCollection>();
                            cardData.PassiveEffects[i] = pe;
                        }
                    }

                    if (cardData.RemotePassiveEffects is { Length: > 0 })
                    {
                        for (int i = 0; i < cardData.RemotePassiveEffects.Length; i++)
                        {
                            var rpe = cardData.RemotePassiveEffects[i];
                            var pe = rpe.Effect;
                            pe.DroppedCards ??= Array.Empty<CardsDropCollection>();
                            rpe.Effect = pe;
                            cardData.RemotePassiveEffects[i] = rpe;
                        }
                    }

                    if (cardData.EffectsToContainer is { Length: > 0 })
                    {
                        for (int i = 0; i < cardData.EffectsToContainer.Length; i++)
                        {
                            var rpe = cardData.EffectsToContainer[i];
                            var pe = rpe.Effect;
                            pe.DroppedCards ??= Array.Empty<CardsDropCollection>();
                            rpe.Effect = pe;
                            cardData.EffectsToContainer[i] = rpe;
                        }
                    }
                }
                else if (ProcessingScriptableObjectPack.obj is CharacterPerk perk)
                {
                    if (json.ContainsKey("CharacterPerkPerkGroup") && json["CharacterPerkPerkGroup"].IsString &&
                        !json["CharacterPerkPerkGroup"].ToString().IsNullOrWhiteSpace())
                        WaitForAddPerkGroup.Add(new Tuple<string, CharacterPerk>(
                            json["CharacterPerkPerkGroup"].ToString(), perk));

                    if (!PerkTabGroup_All)
                        PerkTabGroup_All = GameLoad.Instance.DataBase.AllData.OfType<PerkTabGroup>().FirstOrDefault();
                    if (PerkTabGroup_All != null && !PerkTabGroup_All.ContainsPerk(perk))
                    {
                        PerkTabGroup_All.ContainedPerks.Add(perk);
                    }
                }
                else if (ProcessingScriptableObjectPack.obj is GameStat stat)
                {
                    if (json.ContainsKey("VisibleGameStatStatListTab") &&
                        json["VisibleGameStatStatListTab"].IsString &&
                        !json["VisibleGameStatStatListTab"].ToString().IsNullOrWhiteSpace())
                        WaitForAddVisibleGameStat.Add(new Tuple<string, GameStat>(
                            json["VisibleGameStatStatListTab"].ToString(), stat));
                }
                else if (ProcessingScriptableObjectPack.obj is PlayerCharacter character)
                {
                    if (AllGUIDTypeDict.TryGetValue(typeof(Gamemode), out var dict))
                    {
                        foreach (var pair in dict)
                        {
                            var mode = pair.Value as Gamemode;
                            Array.Resize(ref mode.PlayableCharacters,
                                mode.PlayableCharacters.Length + 1);
                            mode.PlayableCharacters[mode.PlayableCharacters.Length - 1] =
                                character;
                        }
                    }

                    WaitForAddJournalPlayerCharacter.Add(new ScriptableObjectPack(character, "", "", "",
                        ProcessingScriptableObjectPack.CardData));

                    character.SunsCost = -1;
                    character.MoonsCost = -1;
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