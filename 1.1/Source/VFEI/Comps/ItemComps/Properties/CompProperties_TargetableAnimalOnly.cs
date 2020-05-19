using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace VFEI
{
    class CompProperties_TargetableAnimalOnly : CompTargetable
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
                return pawn != null && (pawn.RaceProps.Animal || pawn.IsWildMan());
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
