using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class TagOnBoardConditionWarpper : WarpperBase
    {
        public TagOnBoardConditionWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref TagOnBoardCondition obj)
        {
            if (TriggerTagWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.CardTagDict.TryGetValue(TriggerTagWarpData, out var ele) && ele is CardTag)
                {
                    obj.TriggerTag = ele;
                }
            }
        }

        // Object Name
        public String ObjectName;

        // TriggerTag: CardTag
        public WarpperFunction.WarpType TriggerTagWarpType;
        public String TriggerTagWarpData;
    }
}
