using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;
using RimWorld;

namespace VFEI.RaidArrivalModes
{
    class PawnArrivalModeWorker_GigalocustSwarm : PawnsArrivalModeWorker
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
			pawns.Clear();
			Map map = (Map)parms.target;
			int n = (int)parms.points / (int)ThingDefsVFEI.VFEI_Insectoid_Gigalocust.combatPower;
			for (int i = 0; i < n; i++)
			{
				IntVec3 intVec3 = new IntVec3();
				RCellFinder.TryFindRandomCellNearWith(parms.spawnCenter, (c) => c.Walkable(map), map, out intVec3, 4, 20);
				Pawn pawn = PawnGenerator.GeneratePawn(ThingDefsVFEI.VFEI_Insectoid_Gigalocust, parms.faction);
				pawns.Add(pawn);
				SkyfallerMaker.SpawnSkyfaller(ThingDefsVFEI.VFEI_GigalocustIncoming, pawn, intVec3, map);
			}
		}
	}
}
