using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class ExplorationResultWarpper : WarpperBase
    {
        public ExplorationResultWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ExplorationResult obj)
        {
            WarpperFunction.ClassWarpper(obj, "Action", ActionWarpType, ActionWarpData, SrcPath);
        }

        // Object Name
        public String ObjectName;

        // Action: CardAction
        public WarpperFunction.WarpType ActionWarpType;
        public string ActionWarpData;
    }
}
