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
            object box = obj;

            WarpperFunction.ClassWarpper(box, "TriggerTag", TriggerTagWarpType, TriggerTagWarpData, SrcPath);

            obj = (TagOnBoardCondition)box;
        }

        // Object Name
        public String ObjectName;

        // TriggerTag: CardTag
        public WarpperFunction.WarpType TriggerTagWarpType;
        public String TriggerTagWarpData;
    }
}
