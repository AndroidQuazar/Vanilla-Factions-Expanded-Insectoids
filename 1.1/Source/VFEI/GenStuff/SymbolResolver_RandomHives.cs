using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;
using RimWorld.BaseGen;
using RimWorld;
using RimWorld.Planet;

namespace VFEI.GenStuff
{
    class SymbolResolver_RandomHives : SymbolResolver
    {
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			for (int i = 0; i < Rand.RangeInclusive(5, 20); i++)
			{
				Faction fac = rp.faction;
				IntVec3 randomCell = rp.rect.RandomCell;
				if (randomCell.Standable(map) && randomCell.GetFirstItem(map) == null && randomCell.GetFirstPawn(map) == null && randomCell.GetFirstBuilding(map) == null)
				{
					if (Rand.RangeInclusive(1, 4) < 3)
					{
						Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Hive, null), randomCell, map, WipeMode.Vanish);
						hive.SetFaction(fac, null);
						foreach (CompSpawner compSpawner in hive.GetComps<CompSpawner>())
						{
							if (compSpawner.PropsSpawner.thingToSpawn == ThingDefOf.InsectJelly)
							{
								compSpawner.TryDoSpawn();
								break;
							}
						}
					}
					else
					{
						Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefsVFEI.VFEI_LargeHive, null), randomCell, map, WipeMode.Vanish);
						hive.SetFaction(fac, null);
						foreach (CompSpawner compSpawner in hive.GetComps<CompSpawner>())
						{
							if (compSpawner.PropsSpawner.thingToSpawn == ThingDefsVFEI.VFEI_RoyalInsectJelly)
							{
								compSpawner.TryDoSpawn();
								break;
							}
						}
					}
				}
			}
		}
	}
}
