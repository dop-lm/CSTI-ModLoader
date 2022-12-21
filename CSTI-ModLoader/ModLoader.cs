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
        public static Dictionary<string, CardTag> CardTagDict = new Dictionary<string, CardTag>();
        public static Dictionary<string, EquipmentTag> EquipmentTagDict = new Dictionary<string, EquipmentTag>();
        public static Dictionary<string, ActionTag> ActionTagDict = new Dictionary<string, ActionTag>();
        public static Dictionary<string, ScriptableObject> WaitForWarpperDict = new Dictionary<string, ScriptableObject>();
        public static Dictionary<string, CardTabGroup> CardTabGroupDict = new Dictionary<string, CardTabGroup>();

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
            for (int i = 0; i < GameLoad.Instance.DataBase.AllData.Count; i++)
            {
                if (!AllGUIDDict.ContainsKey(GameLoad.Instance.DataBase.AllData[i].UniqueID))
                    AllGUIDDict.Add(GameLoad.Instance.DataBase.AllData[i].UniqueID, GameLoad.Instance.DataBase.AllData[i]);
                else
                    UnityEngine.Debug.LogWarning("AllGUIDDict Same Key was Add " + GameLoad.Instance.DataBase.AllData[i].UniqueID);
            }
            foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(CardTag)))
            {
                if (!CardTagDict.ContainsKey(ele.name))
                    CardTagDict.Add(ele.name, ele as CardTag);
                else
                    UnityEngine.Debug.LogWarning("CardTagDict Same Key was Add " + ele.name);
            }
            foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(EquipmentTag)))
            {
                if (!EquipmentTagDict.ContainsKey(ele.name))
                    EquipmentTagDict.Add(ele.name, ele as EquipmentTag);
                else
                    UnityEngine.Debug.LogWarning("EquipmentTagDict Same Key was Add " + ele.name);
            }
            foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(ActionTag)))
            {
                if (!ActionTagDict.ContainsKey(ele.name))
                    ActionTagDict.Add(ele.name, ele as ActionTag);
                else
                    UnityEngine.Debug.LogWarning("ActionTagDict Same Key was Add " + ele.name);
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
            foreach (var ele in Resources.FindObjectsOfTypeAll(typeof(CardTabGroup)))
            {
                if (!CardTabGroupDict.ContainsKey(ele.name))
                    CardTabGroupDict.Add(ele.name, ele as CardTabGroup);
                else
                    UnityEngine.Debug.LogWarning("CardTabGroupDict Same Key was Add " + ele.name);
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
