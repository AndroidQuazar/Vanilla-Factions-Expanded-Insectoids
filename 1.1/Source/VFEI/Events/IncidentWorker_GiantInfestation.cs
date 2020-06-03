using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using UnityEngine;

namespace VFEI.Events
{
    class IncidentWorker_GiantInfestation : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec;
			return base.CanFireNowSub(parms) && HiveUtility.TotalSpawnedHivesCount(map) < 30 && TryFindCell(out intVec, map);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Thing t = SpawnTunnels(Mathf.Max(GenMath.RoundRandom(parms.points / 120f), 1), map, false, false, null);
			base.SendStandardLetter(parms, t, Array.Empty<NamedArgument>());
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}

		private static bool CheckRepeller(Map map, IntVec3 cell)
		{
			foreach (Building building in map.listerBuildings.AllBuildingsColonistOfDef(DefDatabase<ThingDef>.GetNamed("VFEI_SonicInfestationRepeller")).ToList())
			{
				if (IntVec3Utility.ManhattanDistanceFlat(cell, building.Position) < 50)
				{
					return false;
				}
			}
			return true;
		}

		public static bool TryFindCell(out IntVec3 cell, Map map)
		{
			cell = CellFinderLoose.RandomCellWith(
				(i) => !i.Fogged(map) && i.DistanceToEdge(map) < 25 && i.GetRoof(map) == RoofDefOf.RoofRockThick && i.Walkable(map) && i.GetTemperature(map) > -17f && CheckRepeller(map, i) && i.InBounds(map),
				map);
			if (!cell.InBounds(map))
			{
				return false;
			}
			return true;
		}

		public static Thing SpawnTunnels(int hiveCount, Map map, bool spawnAnywhereIfNoGoodCell = false, bool ignoreRoofedRequirement = false, string questTag = null)
		{
			IntVec3 loc;
			TryFindCell(out loc, map);
			Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner, null), loc, map, WipeMode.FullRefund);
			QuestUtility.AddQuestTag(thing, questTag);
			for (int i = 0; i < hiveCount - 1; i++)
			{
				loc = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, ThingDefOf.Hive, ThingDefOf.Hive.GetCompProperties<CompProperties_SpawnerHives>(), ignoreRoofedRequirement, true);
				if (loc.IsValid)
				{
					thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner, null), loc, map, WipeMode.FullRefund);
					QuestUtility.AddQuestTag(thing, questTag);
				}
			}
			return thing;
		}
	}
}
