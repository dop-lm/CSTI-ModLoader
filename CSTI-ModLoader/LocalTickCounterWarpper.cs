﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class LocalTickCounterWarpper : WarpperBase
    {
        public LocalTickCounterWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(LocalTickCounter obj)
        {
        }

        // Object Name
        public String ObjectName;
    }
}