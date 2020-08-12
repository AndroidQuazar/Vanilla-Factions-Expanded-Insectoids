using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;
using RimWorld.BaseGen;
using RimWorld;
using RimWorld.Planet;

namespace VFEI.GenStuff
{
    class SymbolResolver_RandomDamage : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			map.listerThings.AllThings.FindAll(t1 => t1.Faction == Find.FactionManager.FirstFactionOfDef(ThingDefsVFEI.VFEI_Insect)).ForEach(t => t.HitPoints -= t.HitPoints / Rand.RangeInclusive(3, 10));
			map.listerThings.AllThings.FindAll(t2 => t2.def.IsMeat || t2.def.defName == "Pemmican").ForEach(t => t.DeSpawn());
		}
	}
}
