﻿using System;
using System.IO;
using System.Text;
using BepInEx;
using LitJson;
using UnityEngine;

namespace ModLoader.LoaderUtil
{
    public static class LoadPreData
    {
        public static void LoadFromPreLoadData()
        {
            try
            {
                foreach (var task in ModLoader.uniqueObjWaitList)
                {
                    task.Wait();
                    var (uniqueObjs, modName) = task.Result;
                    foreach (var (dat, pat, type) in uniqueObjs)
                    {
                        string CardName = Path.GetFileNameWithoutExtension(pat);
                        string CardPath = pat;
                        try
                        {
                            var CardData = Encoding.UTF8.GetString(dat);
                            JsonData json = JsonMapper.ToObject(CardData);

                            if (!(json.ContainsKey("UniqueID") && json["UniqueID"].IsString &&
                                  !json["UniqueID"].ToString().IsNullOrWhiteSpace()))
                            {
                                Debug.LogErrorFormat(
                                    "{0} EditorLoadZip {1} {2} try to load a UniqueIDScriptable without GUID",
                                    type.Name, modName, CardName);
                                continue;
                            }

                            var card = ScriptableObject.CreateInstance(type) as UniqueIDScriptable;
                            // JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(card), card);
                            JsonUtility.FromJsonOverwrite(CardData, card);

                            card.name = modName + "_" + CardName;
                            //type.GetMethod("Init", bindingFlags, null, new Type[] { }, null).Invoke(card, null);

                            var card_guid = card.UniqueID;
                            ModLoader.AllGUIDDict.Add(card_guid, card);
                            GameLoad.Instance.DataBase.AllData.Add(card);

                            if (!ModLoader.WaitForWarpperEditorGuidDict.ContainsKey(card_guid))
                                ModLoader.WaitForWarpperEditorGuidDict.Add(card_guid,
                                    new ModLoader.ScriptableObjectPack(card, "", CardPath, modName,
                                        CardData));
                            else
                                Debug.LogWarningFormat(
                                    "{0} WaitForWarpperEditorGuidDict Same Key was Add {1}", modName, card_guid);
                            if (!ModLoader.AllScriptableObjectDict.ContainsKey(card_guid))
                                ModLoader.AllScriptableObjectDict.Add(card_guid, card);
                            if (ModLoader.AllGUIDTypeDict.TryGetValue(type, out var dict))
                                if (!dict.ContainsKey(card_guid))
                                    dict.Add(card_guid, card);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarningFormat("{0} EditorLoad {1} {2} Error {3}", type.Name, modName,
                                CardName, ex.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        
        public static void LoadData(string mods_dir)
        {
            try
            {
                var dirs = Directory.GetDirectories(mods_dir);
                foreach (var dir in dirs)
                {
                    //  Check if is a Mod Directory
                    if (!File.Exists(ModLoader.CombinePaths(dir, "ModInfo.json")))
                        continue;

                    ModInfo Info = new ModInfo();
                    string ModName = Path.GetFileName(dir);

                    try
                    {
                        // Load Mod Info
                        using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(dir, "ModInfo.json")))
                            JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), Info);

                        // Check Name
                        if (!Info.Name.IsNullOrWhiteSpace())
                            ModName = Info.Name;

                        ModLoader.ModPacks[ModName] = new ModPack(Info, ModName,
                            ModLoader.Instance.Config.Bind("是否加载某个模组",
                                $"{ModName}_{Info.Name}".EscapeStr(), true,
                                $"是否加载{ModName}"));
                        if (!ModLoader.ModPacks[ModName].EnableEntry.Value) continue;

                        Debug.Log($"ModLoader PreLoad Mod {ModName} {Info.Version}");

                        // Check Verison
                        var ModRequestVersion = Version.Parse(Info.ModLoaderVerison);
                        if (ModLoader.PluginVersion.CompareTo(ModRequestVersion) < 0)
                            Debug.LogWarningFormat(
                                "ModLoader Version {0} is lower than {1} Request Version {2}", ModLoader.PluginVersion, ModName,
                                ModRequestVersion);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarningFormat("{0} Check Version Error {1}", ModName, ex);
                    }

                    // Load Pictures
                    try
                    {
                        var picPath = ModLoader.CombinePaths(dir, "Resource", "Picture");
                        if (Directory.Exists(picPath))
                        {
                            var files = Directory.GetFiles(picPath);
                            PostSpriteLoad.SpriteLoadQueue.Enqueue(ResourceLoadHelper.LoadPictures(ModName, files));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarningFormat("{0} Load Pictures Error {1}", ModName, e);
                    }

                    // Load and init UniqueIDScriptable
                    try
                    {
                        ModLoader.uniqueObjWaitList.Add(
                            ResourceLoadHelper.LoadUniqueObjs(ModName, dir, ModLoader.GameSourceAssembly, Info));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarningFormat("{0} Load UniqueIDScriptable Error {1}", ModName,
                            ex);
                    }
                }
            }
            catch (Exception e)
            {
                ModLoader.Instance.CommonLogger.LogError($"loading error :{e}");
            }
            finally
            {
                PostSpriteLoad.NoMoreSpriteLoadQueue = true;
            }
        }

    }
}