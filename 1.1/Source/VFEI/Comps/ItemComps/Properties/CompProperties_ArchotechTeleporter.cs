using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace VFEI
{
    class CompProperties_ArchotechTeleporter : CompProperties
    {
		public CompProperties_ArchotechTeleporter()
		{
			this.compClass = typeof(VFEI.Comps.ItemComps.CompArchotechTeleporter);
		}
	}
}
