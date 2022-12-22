using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class TimeObjectiveWarpper : WarpperBase
    {
        public TimeObjectiveWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(TimeObjective obj)
        {
        }

        // Object Name
        public String ObjectName;
    }
}
