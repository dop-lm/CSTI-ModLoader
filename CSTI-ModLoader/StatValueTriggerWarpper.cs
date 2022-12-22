using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class StatValueTriggerWarpper : WarpperBase
    {
        public StatValueTriggerWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref StatValueTrigger obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "Stat", StatWarpType, StatWarpData, SrcPath);

            obj = (StatValueTrigger)box;
        }

        // Stat: GameStat
        public WarpperFunction.WarpType StatWarpType;
        public String StatWarpData;
    }
}
