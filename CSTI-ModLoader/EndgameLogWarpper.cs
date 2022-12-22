using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class EndgameLogWarpper : WarpperBase
    {
        public EndgameLogWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(EndgameLog obj)
        {
            WarpperFunction.ClassWarpper(obj, "Category", CategoryWarpType, CategoryWarpData, SrcPath);
        }

        // Category: EndgameLogCategory
        public WarpperFunction.WarpType CategoryWarpType;
        public string CategoryWarpData;
    }
}
