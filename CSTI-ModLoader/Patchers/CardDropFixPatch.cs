using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ModLoader.Patchers;

[HarmonyPatch]
public static class CardDropFixPatch
{
    [HarmonyPostfix, HarmonyPatch(typeof(CardData), nameof(CardData.Init))]
    public static void InitPost(CardData __instance)
    {
        foreach (var dropCollection in __instance.AllDrops)
        {
            if (dropCollection == null) continue;
            var droppedCards = dropCollection.DroppedCards;
            if (droppedCards == null || droppedCards.All(drop => drop.DroppedCard))
            {
                return;
            }

            dropCollection.DroppedCards = droppedCards.Where(drop => drop.DroppedCard).ToArray();
        }
    }
}