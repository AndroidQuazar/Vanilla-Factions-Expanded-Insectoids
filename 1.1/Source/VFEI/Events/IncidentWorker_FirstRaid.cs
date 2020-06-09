using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace VFEI.Events
{
    class IncidentWorker_FirstRaid : IncidentWorker
    {
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			return false;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			PlayerKnowledgeDatabase.SetKnowledge(ConceptDefOf.ShieldBelts, 1f);
			Map map1 = (Map)parms.target;
			IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map1);
			incidentParms.faction = Find.FactionManager.AllFactionsVisible.Where((f) => f.def.defName == "VFEI_Insect").First();
			incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
			incidentParms.points = incidentParms.points * 10;
			//Log.Message("Faction: " + incidentParms.faction.ToString());
			//Log.Message("RaidStrategy: " + incidentParms.raidStrategy.ToString());
			//Log.Message("RaidArrivalMode: " + incidentParms.raidArrivalMode.ToString());
			//Log.Message("Points: " + incidentParms.points.ToString());
			//Log.Message("PawnCount: " + incidentParms.pawnCount.ToString());
			//Log.Message("SpawnCenter: " + incidentParms.spawnCenter.ToString());
			//Log.Message("Target: " + incidentParms.target.ToString());
			Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame, incidentParms);
			return true;
		}
	}
}
