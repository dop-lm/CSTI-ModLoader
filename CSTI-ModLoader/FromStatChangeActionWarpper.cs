using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class FromStatChangeActionWarpper : CardActionWarpper
    {
        public FromStatChangeActionWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(FromStatChangeAction obj)
        {   
            base.WarpperCustomSelf(obj);

            WarpperFunction.ClassWarpper(obj, "StatChangeTrigger", StatChangeTriggerWarpType, StatChangeTriggerWarpData, SrcPath);
        }

        // StatChangeTrigger: StatValueTrigger[]
        public WarpperFunction.WarpType StatChangeTriggerWarpType;
        public List<String> StatChangeTriggerWarpData;
    }
}
