using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI.Group;

namespace VFEI.RaidArrivalModes
{
    class PawnsArrivalModeWorker_Meteorite : PawnsArrivalModeWorker
    {
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			InsectTunnel(parms, pawns);
		}

		public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!parms.spawnCenter.IsValid)
			{
				parms.spawnCenter = CellFinderLoose.RandomCellWith((i) => i.Walkable(map) == true, map);
			}
			parms.spawnRotation = Rot4.Random;
			return true;
		}

		public static void InsectTunnel(IncidentParms parms, List<Pawn> pawns)
		{
			Map map = (Map)parms.target;
			int numberOfMeteor = Rand.RangeInclusive(2, (int) parms.points / 500);
			int pawnsPerMeteor = (int)pawns.Count / numberOfMeteor;
			foreach (Pawn item in pawns)
			{
				if (item.GetLord() != null)
				{
					item.GetLord().Cleanup();
				}
			}
			LordMaker.MakeNewLord(parms.faction, new LordJob_AssaultColony(parms.faction, false, false, false, false, false), map, pawns);
			IntVec3 intVec3 = new IntVec3();
			for (int n = 0; n < numberOfMeteor; n++)
			{
				intVec3 = CellFinderLoose.RandomCellWith((i) => i.Walkable(map), map);
				List<Thing> pawnsM = ThingSetMakerDefOf.Meteorite.root.Generate();
				for (int i = 0; i < pawnsPerMeteor; i++)
				{
					if (pawns[0] != null)
					{
						pawnsM.Add(pawns[0]);
						pawns.RemoveAt(0);
					}
					else break;
				}
				SkyfallerMaker.SpawnSkyfaller(ThingDefsVFEI.VFEI_InsectMeteoriteIncoming, pawnsM, intVec3, map);
			}
			parms.spawnCenter = intVec3;
		}
	}
}
