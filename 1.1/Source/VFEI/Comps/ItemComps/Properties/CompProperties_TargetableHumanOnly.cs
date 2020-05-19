using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace VFEI
{
    class CompProperties_TargetableHumanOnly : CompTargetable
    {
        protected override bool PlayerChoosesTarget
        {
            get
            {
                return true;
            }
        }

        protected override TargetingParameters GetTargetingParameters()
        {
            TargetingParameters targetingParameters = new TargetingParameters();
            targetingParameters.validator = delegate (TargetInfo targ)
            {
                if (!base.BaseTargetValidator(targ.Thing))
                {
                    return false;
                }
                Pawn pawn = targ.Thing as Pawn;
                return pawn != null && (pawn.RaceProps.Humanlike && !pawn.IsWildMan() && !pawn.IsColonist);
            };
            targetingParameters.canTargetPawns = true;
            targetingParameters.canTargetBuildings = false;
            return targetingParameters;
        }

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
            yield break;
        }
    }
}