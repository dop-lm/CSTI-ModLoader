using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class SelfTriggeredActionWarpper : WarpperBase
    {
        public SelfTriggeredActionWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(SelfTriggeredAction obj)
        {
            WarpperFunction.ClassWarpper(obj, "Actions", ActionsWarpType, ActionsWarpData, SrcPath);
        }

        // Actions: FromStatChangeAction[]
        public WarpperFunction.WarpType ActionsWarpType;
        public List<String> ActionsWarpData;
    }
}
