using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class BlueprintStageWarpper : WarpperBase
    {
        public BlueprintStageWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(BlueprintStage obj)
        {
            WarpperFunction.ClassWarpper(obj, "RequiredElements", RequiredElementsWarpType, RequiredElementsWarpData, SrcPath);
        }

        // Object Name
        public String ObjectName;

        // RequiredElements: BlueprintElement[]
        public WarpperFunction.WarpType RequiredElementsWarpType;
        public List<string> RequiredElementsWarpData;
    }
}
