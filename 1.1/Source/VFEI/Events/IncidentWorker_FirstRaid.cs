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
			Map map1 = (Map)parms.target;
			IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map1);
			incidentParms.faction = Find.FactionManager.AllFactionsVisible.Where((f) => f.def.defName == "VFEI_Insect").First();
			incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			incidentParms.points = incidentParms.points * 4;
			Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame, incidentParms);
			return true;
		}
	}
}
