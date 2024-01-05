using System;
using LitJson;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ModLoader.TestTool;

public static class LoaderTimeTest
{
    public static void SimpleTest1()
    {
        var cards = Resources.FindObjectsOfTypeAll<CardData>();
        var cardData = ScriptableObject.CreateInstance<CardData>();
        var dateTime1 = DateTime.Now;
        for (var i = 0; i < 100; i++)
        {
            JsonUtility.FromJsonOverwrite(
                JsonMapper.ToObject(JsonUtility.ToJson(cards[Random.Range(0, cards.Length)])).ToJson(), cardData);
        }

        var dateTime2 = DateTime.Now;
        Debug.Log($"cost time:{dateTime2 - dateTime1:g}");
    }

    public static void SimpleTest2()
    {
        var cards = Resources.FindObjectsOfTypeAll<CardData>();
        var cardData = ScriptableObject.CreateInstance<CardData>();
        var dateTime1 = DateTime.Now;
        for (var i = 0; i < 100; i++)
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(cards[Random.Range(0, cards.Length)]), cardData);
        }

        var dateTime2 = DateTime.Now;
        Debug.Log($"cost time:{dateTime2 - dateTime1:g}");
    }
}