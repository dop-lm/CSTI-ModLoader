using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class ObjectiveSubObjectiveWarpper : WarpperBase
    {
        public ObjectiveSubObjectiveWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ObjectiveSubObjective obj)
        {
            WarpperFunction.ClassWarpper(obj, "ObjectiveCondition", ObjectiveConditionWarpType, ObjectiveConditionWarpData, SrcPath);
        }

        // ObjectiveCondition: Objective
        public WarpperFunction.WarpType ObjectiveConditionWarpType;
        public string ObjectiveConditionWarpData;
    }
}
