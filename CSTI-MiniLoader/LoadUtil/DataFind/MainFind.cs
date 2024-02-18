using System.Collections.Generic;
using UnityEngine;

namespace CSTI_MiniLoader.LoadUtil.DataFind;

public static class MainFind
{
    public static IEnumerable<Object> Find(this UniqueIDScriptable idScriptable)
    {
        if (idScriptable is CardData cardData)
        {
            foreach (var r in cardData.ActiveCounters)
            {
                yield return r;
            }

            foreach (var cardTag in cardData.CardTags)
            {
                yield return cardTag;
            }

            foreach (var equipmentTag in cardData.EquipmentTags)
            {
                yield return equipmentTag;
            }

            yield return cardData.CardImage;
        }

        if (idScriptable is GameStat gameStat)
        {
            yield return gameStat.GetIcon;
            foreach (var status in gameStat.Statuses)
            {
                yield return status.Icon;
                foreach (var au in status.AlertSounds)
                {
                    yield return au;
                }
            }
        }
    }
}