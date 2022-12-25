using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{

    public class StatusEndgameLogWarpper : EndgameLogWarpper
    {
        public StatusEndgameLogWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(StatusEndgameLog obj)
        {
            base.WarpperCustomSelf(obj);
        }

    }
}
