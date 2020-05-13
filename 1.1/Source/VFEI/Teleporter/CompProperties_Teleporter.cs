using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace VFEI
{
    class CompProperties_Teleporter : CompProperties
    {
        public CompProperties_Teleporter()
        {
            this.compClass = typeof(CompTeleporter);
        }
    }
}
