using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;

namespace VFEI.PawnComps
{
    public class CompProperties_ThoughtEffecter : CompProperties
    {
        public int radius = 1;
        public int tickInterval = 1000;
        public string thoughtDef = "AteWithoutTable";
        public bool showEffect = false;

        public CompProperties_ThoughtEffecter()
        {
            this.compClass = typeof(CompThoughtEffecter);
        }


    }
}
