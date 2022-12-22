using BepInEx;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace ModLoader
{

    [BepInPlugin("Dop.plugin.CSTI.ModLoader", "ModLoader", "1.0.0")]
    public class ModLoader : BaseUnityPlugin
    {
        public static Dictionary<string, UnityEngine.Sprite> SpriteDict = new Dictionary<string, UnityEngine.Sprite>();
        public static Dictionary<string, UnityEngine.AudioClip> AudioClipDict = new Dictionary<string, UnityEngine.AudioClip>();

        public static Dictionary<string, UniqueIDScriptable> AllGUIDDict = new Dictionary<string, UniqueIDScriptable>();
        public static Dictionary<string, ScriptableObject> AllScriptableObjectDict = new Dictionary<string, ScriptableObject>();
        public static Dictionary<string, CardTag> CardTagDict = new Dictionary<string, CardTag>();
        public static Dictionary<string, EquipmentTag> EquipmentTagDict = new Dictionary<string, EquipmentTag>();
        public static Dictionary<string, ActionTag> ActionTagDict = new Dictionary<string, ActionTag>();
        public static Dictionary<string, ScriptableObject> WaitForWarpperDict = new Dictionary<string, ScriptableObject>();
        public static Dictionary<string, CardTabGroup> CardTabGroupDict = new Dictionary<string, CardTabGroup>();
        public static Dictionary<string, EndgameLogCategory> EndgameLogCategoryDict = new Dictionary<string, EndgameLogCategory>();

        private void Awake()
        {
            // Plugin startup logic
            Harmony.CreateAndPatchAll(typeof(ModLoader));
            Logger.LogInfo("Plugin ModLoader is loaded!");

            AssetBundle ab = AssetBundle.LoadFromFile(BepInEx.Paths.BepInExRootPath + "\\plugins\\BambooTech\\Resource\\bambootech.ab");
            SpriteDict.Add("BambooGrove", ab.LoadAsset<UnityEngine.Sprite>("BambooGrove"));
            ab.Unload(false);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameLoad), "LoadGameData")]
        public static void GameLoadLoadGameDataPostfix()
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
                else if (ele is EndgameLogCategory)
                {
                    if (!EndgameLogCategoryDict.ContainsKey(ele.name))
                        EndgameLogCategoryDict.Add(ele.name, ele as EndgameLogCategory);
                    else
                        UnityEngine.Debug.LogWarning("EndgameLogCategoryDict Same Key was Add " + ele.name);
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

            // find all mod and init all data
            CardData card = CardData.CreateInstance<CardData>();
            using (StreamReader sr = new StreamReader("D:\\BambooTech\\CardData\\BambooGrove\\BambooGrove.json"))
                JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), card);card.name = "BambooGrove";
            card.name = "BambooGrove";
            card.Init();
            AllGUIDDict.Add(card.UniqueID, card);
            GameLoad.Instance.DataBase.AllData.Add(card);
            WaitForWarpperDict.Add(card.name, card);

            foreach (var item in WaitForWarpperDict)
            {
                if (item.Value is CardData)
                {
                    CardDataWarpper warpper = new CardDataWarpper("D:\\BambooTech\\CardData\\" + item.Value.name);
                    using (StreamReader sr = new StreamReader("D:\\BambooTech\\CardData\\" + item.Value.name + "\\" + item.Value.name + ".json"))
                        JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), warpper);
                    warpper.WarpperCustomSelf(card);
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CheatsManager), "CheatsActive", MethodType.Getter)]
        public static void CheatsManagerCheatsActive(ref bool __result)
        {
            __result = true;
        }

    }
}
