using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class LocalCounterEffectWarpper : WarpperBase
    {
        public LocalCounterEffectWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref LocalCounterEffect obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "Counter", CounterWarpType, CounterWarpData, SrcPath);

            obj = (LocalCounterEffect)box;
        }

        // Object Name
        public String ObjectName;

        // Counter: LocalTickCounter
        public WarpperFunction.WarpType CounterWarpType;
        public String CounterWarpData;
    }
}
