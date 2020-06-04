using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

namespace VFEI
{
    class VFEICocoon : ThingWithComps
    {
		int timeBeforeInsect;
		int timeBeforeInsectString;
		bool once = true;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.timeBeforeInsect, "timeBeforeInsect");
			Scribe_Values.Look<int>(ref this.timeBeforeInsectString, "timeBeforeInsectString");
			Scribe_Values.Look<bool>(ref this.once, "onceCocoonDev");
		}

		public override string GetInspectString()
		{
			return "CocoonInsectSpawnIn".Translate(timeBeforeInsectString.ToStringTicksToPeriod());
		}

		public override void Tick()
		{
			base.Tick();
			if (once) { this.timeBeforeInsect = Find.TickManager.TicksGame + 15000; timeBeforeInsectString = 15000; once = false; }
			if (Find.TickManager.TicksGame == this.timeBeforeInsect)
			{
				IntVec3 pos = this.Position;
				Map map = this.Map;

				IntVec3 c;
				CellFinder.TryFindRandomReachableCellNear(pos, map, 4, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
				FilthMaker.TryMakeFilth(c, map, ThingDefOf.Filth_Slime);
				SoundDefOf.Hive_Spawn.PlayOneShot(new TargetInfo(pos, map));

				List<PawnKindDef> pawnKindDefs = new List<PawnKindDef>();
				pawnKindDefs.Add(PawnKindDefOf.Megascarab);
				pawnKindDefs.Add(PawnKindDefOf.Spelopede);
				pawnKindDefs.Add(PawnKindDefOf.Megaspider);
				pawnKindDefs.Add(ThingDefsVFEI.VFEI_Insectoid_Megapede);
				pawnKindDefs.Add(ThingDefsVFEI.VFEI_Insectoid_Gigalocust);
				pawnKindDefs.Add(ThingDefsVFEI.VFEI_Insectoid_RoyalMegaspider);
				pawnKindDefs.Add(ThingDefsVFEI.VFEI_Insectoid_Queen);

				if (pawnKindDefs.Count > 0)
				{
					PawnKindDef pawnKind = pawnKindDefs.RandomElementByWeight(x => x.combatPower / x.race.BaseMarketValue);
					List<Faction> factions = new List<Faction>();
					FactionManager.GetInViewOrder(factions);
					Faction fac = factions.Where((Faction f) => f.def.defName == "VFEI_Insect").RandomElement();
					Pawn p = PawnGenerator.GeneratePawn(pawnKind, fac);
					GenSpawn.Spawn(p, pos, map);
					List<Pawn> pawns = new List<Pawn>();
					pawns.Add(p);
					if (map.ParentFaction == fac)
					{
						LordMaker.MakeNewLord(fac, new LordJob_DefendBase(fac, map.Center), map, pawns);
					}
				}
				this.Destroy();
			}
			timeBeforeInsectString--;
		}
	}
}
