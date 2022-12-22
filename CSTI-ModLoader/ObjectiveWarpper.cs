using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class ObjectiveWarpper : WarpperBase
    {
        public ObjectiveWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(Objective obj)
        {
        }

        // Object Name
        public String ObjectName;
    }
}
