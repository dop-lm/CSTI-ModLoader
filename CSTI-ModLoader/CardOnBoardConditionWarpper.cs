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
            object box = obj;

            WarpperFunction.ClassWarpper(box, "TriggerCard", TriggerCardWarpType, TriggerCardWarpData, SrcPath);

            obj = (CardOnBoardCondition)box;
        }

        // Object Name
        public String ObjectName;

        // TriggerCard: CardData
        public WarpperFunction.WarpType TriggerCardWarpType;
        public String TriggerCardWarpData;
    }
}
