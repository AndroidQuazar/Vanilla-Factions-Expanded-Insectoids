using System;
using RimWorld;
using Verse;

namespace VFEI
{
    public class CompTargetEffect_Tame : CompTargetEffect
    {
        public int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        public override void DoEffectOn(Pawn user, Thing target)
        {
            Pawn pawn = (Pawn)target;
            if (pawn.AnimalOrWildMan() && pawn.Faction == null && pawn.RaceProps.wildness < 1f && !pawn.IsPrisonerInPrisonCell())
            {
                if (pawn.Dead)
                {
                    return;
                }
                if (this.RandomNumber(1, 10) == 1)
                {
                    pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, true, false, null, false);

                    string text;
                    text = "LetterArtifactTameFail".Translate(pawn).CapitalizeFirst();
                    Find.LetterStack.ReceiveLetter("LetterLabelForcedTameFail".Translate(pawn.KindLabel, pawn).CapitalizeFirst(), text, LetterDefOf.NegativeEvent, pawn, null, null);
                }
                else
                {
                    if (pawn.guest != null)
                    {
                        pawn.guest.SetGuestStatus(null, false);
                    }
                    string value = pawn.LabelIndefinite();
                    bool flag = pawn.Name != null;
                    pawn.SetFaction(Faction.OfPlayer, null);
                    string text;
                    #pragma warning disable 0618
                    if (pawn.kindDef == PawnKindDefOf.WildMan)
                    {
                        text = "LetterArtifactTame".Translate(pawn.Name).CapitalizeFirst();
                        Find.LetterStack.ReceiveLetter("LetterLabelForcedTame".Translate(pawn.Name, pawn).CapitalizeFirst(), text, LetterDefOf.PositiveEvent, pawn, null, null);
                    }
                    else
                    {
                        text = "LetterArtifactTame".Translate(pawn).CapitalizeFirst();
                        Find.LetterStack.ReceiveLetter("LetterLabelForcedTame".Translate(pawn.KindLabel, pawn).CapitalizeFirst(), text, LetterDefOf.PositiveEvent, pawn, null, null);
                    }               
                }
            }
            else
            {
                return;
            }

        }
    }
}
