using System;
using RimWorld;
using Verse;

namespace VFEI.Comps
{
    class CompProperties_Armillarix : CompProperties
    {
		public CompProperties_Armillarix()
		{
			this.compClass = typeof(CompArmillarix);
		}
	}

    class CompArmillarix : ThingComp
    {
		public CompProperties_Armillarix Props
		{
			get
			{
				return (CompProperties_Armillarix)this.props;
			}
		}

		public override void CompTick()
		{
			if (Find.TickManager.TicksGame % 500 == 0)
			{
				Plant p = this.parent as Plant;
				if (p.LifeStage == PlantLifeStage.Mature)
				{
					this.parent.BroadcastCompSignal("armillarixOn");
				}
				else
				{
					this.parent.BroadcastCompSignal("armillarixOff");
				}
			}
		}
	}
}
