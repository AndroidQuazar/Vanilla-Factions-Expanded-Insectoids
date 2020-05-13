using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;
using RimWorld.BaseGen;
using RimWorld;
using RimWorld.Planet;

namespace VFEI.GenStuff
{
    class SymbolResolver_EdgeRocksIbase : SymbolResolver
    {
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			foreach (IntVec3 intVec in rp.rect.EdgeCells)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Granite);
				thing.SetFaction(rp.faction, null);
				GenSpawn.Spawn(thing, intVec, map, WipeMode.Vanish);
				/*for (int i = 0; i < 8; i++)
				{
					IntVec3 intVec3Copy = new IntVec3();
					RCellFinder.TryFindRandomCellNearWith(intVec, (intv) => intv.Walkable(map), map, out intVec3Copy, 1, 3);
					GenSpawn.Spawn(thing, intVec3Copy, map, WipeMode.Vanish);
				}*/
			}
		}
	}
}
