using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace VFEI
{
    class IngestionOutcomeDoer_Immunity : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            if (pawn.health != null && pawn.health.hediffSet.HasImmunizableNotImmuneHediff() && pawn.health.hediffSet.HasHediff(HediffDefOf.WoundInfection) && pawn.health.immunity != null && pawn.health.immunity.GetImmunity(HediffDefOf.WoundInfection) != 1) {
                ImmunityRecord cPawn = pawn.health.immunity.GetImmunityRecord(HediffDefOf.WoundInfection);
                cPawn.immunity += (this.percent / 100) * ingested.stackCount;
            }
        }
        #pragma warning disable 0649
        public float percent = 0.05f;
    }
}
