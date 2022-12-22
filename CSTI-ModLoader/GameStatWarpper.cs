using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    internal class GameStatWarpper : WarpperBase
    {
        public GameStatWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(GameStat obj)
        {
        }



        // Object Name
        public String ObjectName;
    }
}
