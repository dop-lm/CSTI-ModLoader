using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardDropWarpper : WarpperBase
    {
        public CardDropWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref CardDrop obj)
        {
            if (DroppedCardWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.AllGUIDDict.TryGetValue(DroppedCardWarpData, out var ele) && ele is CardData)
                {
                    obj.DroppedCard = ele as CardData;
                }
            }
        }

        // Object Name
        public String ObjectName;

        // DroppedCard: CardData
        public WarpperFunction.WarpType DroppedCardWarpType;
        public string DroppedCardWarpData;
    }

}
