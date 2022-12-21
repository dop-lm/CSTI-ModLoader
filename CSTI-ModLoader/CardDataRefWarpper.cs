using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardDataRefWarpper : WarpperBase
    {
        public CardDataRefWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(CardDataRef obj)
        {
            if (CardWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.AllGUIDDict.TryGetValue(CardWarpData, out var ele) && ele is CardData)
                    obj.Card = ele as CardData;
            }
        }

        // Object Name
        public String ObjectName;

        // Card: CardData
        public WarpperFunction.WarpType CardWarpType;
        public String CardWarpData;
    }
}
