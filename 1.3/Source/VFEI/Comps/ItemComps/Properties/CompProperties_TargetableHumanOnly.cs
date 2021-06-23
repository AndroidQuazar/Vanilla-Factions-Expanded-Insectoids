using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFEI
{
    internal class CompProperties_TargetableHumanOnly : CompTargetable
    {
        protected override bool PlayerChoosesTarget
        {
            get
            {
                return true;
            }
        }

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
            yield break;
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
    }
}