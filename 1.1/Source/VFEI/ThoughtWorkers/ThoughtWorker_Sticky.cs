using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;

namespace VFEI.ThoughtWorkers
{
    class ThoughtWorker_Sticky : ThoughtWorker
    {
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
            if (p.health.hediffSet.HasHediff(ThingDefsVFEI.VFEI_PheromoneSecretor))
            {
                return true;
            }
            return false;
        }
	}
}
