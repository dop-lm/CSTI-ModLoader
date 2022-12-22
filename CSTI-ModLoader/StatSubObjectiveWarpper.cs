using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class StatSubObjectiveWarpper : WarpperBase
    {
        public StatSubObjectiveWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(StatSubObjective obj)
        {
            WarpperFunction.ClassWarpper(obj, "StatCondition", StatConditionWarpType, StatConditionWarpData, SrcPath);
        }

        // Object Name
        public String ObjectName;

        // StatCondition: StatValueTrigger
        public WarpperFunction.WarpType StatConditionWarpType;
        public string StatConditionWarpData;
    }
}
