using System;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace VFEI
{
    public class CompTargetEffect_Recruit : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            Pawn pawn = (Pawn)target;
            if (!pawn.AnimalOrWildMan() && (pawn.Faction != null || pawn.IsPrisoner))
            {
                if (pawn.Dead)
                {
                    return;
                }
                else
                {
                    if (pawn.guest != null)
                    {
                        pawn.guest.SetGuestStatus(null, false);
                    }
                    Faction oldFac = pawn.Faction;
                    string reason = "GoodWillAffectedByArtifact".Translate();
                    GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(pawn);
                    oldFac.TryAffectGoodwillWith(Faction.OfPlayer, -50, true, true, reason, lookTarget);

                    pawn.SetFaction(Faction.OfPlayer, null);
                    #pragma warning disable 0618
                    string text = "LetterArtifactRecruit".Translate(pawn.Name).CapitalizeFirst();
                    Find.LetterStack.ReceiveLetter("LetterLabelForcedRecruit".Translate(pawn.Name, pawn).CapitalizeFirst(), text, LetterDefOf.PositiveEvent, pawn, null, null);
                }
            }
            else
            {
                return;
            }

        }
    }
}
