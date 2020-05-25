using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;
using RimWorld;

namespace VFEI.RaidStrategyWorkers
{
	public class RaidStrategyWorker_ImmediateAttackInsect : RaidStrategyWorker
	{
		protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
		{
			return new LordJob_AssaultColony(parms.faction, false, false, false, false, false);
		}

		public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
		{
			return base.CanUseWith(parms, groupKind) && parms.faction.def.defName == "VFEI_Insect";
		}

		public override List<Pawn> SpawnThreats(IncidentParms parms)
		{
			return base.SpawnThreats(parms);
		}
	}
}
