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
            if (pawn.health.immunity.GetImmunity(HediffDefOf.WoundInfection) == 1 || !pawn.health.HasHediffsNeedingTend()) { }
            else
            {
                ImmunityRecord cPawn = pawn.health.immunity.GetImmunityRecord(HediffDefOf.WoundInfection);
                cPawn.immunity += (this.percent / 100) * ingested.stackCount;

            }
        }
        public float percent;
    }
}
