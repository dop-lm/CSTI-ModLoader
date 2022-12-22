using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CharacterPerkWarpper : CompletableObjectWarpper
    {
        public CharacterPerkWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(CharacterPerk obj)
        {
            base.WarpperCustomSelf(obj);


        }
    }
}
