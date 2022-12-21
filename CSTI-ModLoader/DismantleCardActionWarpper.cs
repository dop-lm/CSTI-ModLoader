using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ModLoader
{
    public class DismantleCardActionWarpper : CardActionWarpper
    {
        public DismantleCardActionWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(DismantleCardAction obj)
        {
            base.WarpperCustomSelf(obj);
        }
    }
}
