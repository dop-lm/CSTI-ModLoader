using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class LocalTickCounterRefWarpper : WarpperBase
    {
        public LocalTickCounterRefWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(LocalTickCounterRef obj)
        {
            WarpperFunction.ClassWarpper(obj, "Counter", CounterWarpType, CounterWarpData, SrcPath);
        }

        // Object Name
        public String ObjectName;

        // Counter: LocalTickCounter
        public WarpperFunction.WarpType CounterWarpType;
        public String CounterWarpData;
    }
}
