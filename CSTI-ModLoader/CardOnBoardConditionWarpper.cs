using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardOnBoardConditionWarpper : WarpperBase
    {
        public CardOnBoardConditionWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref CardOnBoardCondition obj)
        {
            if (TriggerCardWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.AllGUIDDict.TryGetValue(TriggerCardWarpData, out var ele) && ele is CardData)
                {
                    obj.TriggerCard = ele as CardData;
                }
            }
        }

        // Object Name
        public String ObjectName;

        // TriggerCard: CardData
        public WarpperFunction.WarpType TriggerCardWarpType;
        public String TriggerCardWarpData;
    }
}
