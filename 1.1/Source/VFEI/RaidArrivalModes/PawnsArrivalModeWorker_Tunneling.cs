using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using RimWorld;
using UnityEngine;

namespace VFEI.RaidArrivalModes
{
    class PawnsArrivalModeWorker_Tunneling : PawnsArrivalModeWorker
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
			for (int i = 0; i < 15; i++)
			{
				IntVec3 intvr = new IntVec3();
				RCellFinder.TryFindRandomCellNearWith(parms.spawnCenter, (intv) => intv.Walkable(map), map, out intvr, 1, 4);
				FilthMaker.TryMakeFilth(intvr, map, ThingDefOf.Filth_RubbleRock, 1, FilthSourceFlags.None);
				FilthMaker.TryMakeFilth(intvr, map, ThingDefOf.Filth_Dirt, 1, FilthSourceFlags.None);
				MoteMaker.MakeStaticMote(intvr, map, ThingDefOf.Mote_DustPuffThick, Rand.Range(1.5f, 3f));
			}
			
			foreach (Pawn pawn in pawns)
			{
				GenSpawn.Spawn(pawn, parms.spawnCenter, map);
			}
		}
	}
}
