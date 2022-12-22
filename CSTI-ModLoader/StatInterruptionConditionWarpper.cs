using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class StatInterruptionConditionWarpper : WarpperBase
    {
        public StatInterruptionConditionWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref StatInterruptionCondition obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "Stat", StatWarpType, StatWarpData, SrcPath);

            obj = (StatInterruptionCondition)box;
        }

        // Object Name
        public String ObjectName;

        // Stat: GameStat
        public WarpperFunction.WarpType StatWarpType;
        public String StatWarpData;
    }
}
