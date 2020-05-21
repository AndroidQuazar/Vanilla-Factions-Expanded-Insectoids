using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using UnityEngine;
using Verse;

namespace VFEI.Comps.ItemComps
{
    class CompRandomHediffGiver : HediffComp
    {
        public Properties.CompProperties_RandomHediffGiver Props
        {
            get
            {
                return (Properties.CompProperties_RandomHediffGiver)this.props;
            }
        }

        bool ApplyMutation(RecipeDef recipeDef, out BodyPartRecord bodyPartRecord)
        {
            bodyPartRecord = MedicalRecipesUtility.GetFixedPartsToApplyOn(recipeDef, this.Pawn).RandomElement();
            List<Hediff> hediffs = this.Pawn.health.hediffSet.hediffs;
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                if (hediffs[i].Part == bodyPartRecord && hediffs[i].def.tendable != true)
                {
                    return false;
                }
            }
            return true;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            int MaxTryNumb = 20;
            bool mutate = false;
            for (int i = 0; i < MaxTryNumb; i++)
            {
                RecipeDef randRecipe = this.Props.allowedRecipeDefs.RandomElement();
                BodyPartRecord bodyPartRecord = new BodyPartRecord();
                mutate = ApplyMutation(randRecipe, out bodyPartRecord);
                if (mutate)
                {
                    MaxTryNumb = i;
                    this.Pawn.health.RestorePart(bodyPartRecord);
                    this.Pawn.health.AddHediff(randRecipe.addsHediff, bodyPartRecord);

                    string label1 = "MutationOutcome".Translate();
                    string text1 = "MutationOutcomeLetter".Translate(randRecipe.label.Substring(8));
                    Find.LetterStack.ReceiveLetter(label1, text1, LetterDefOf.NeutralEvent, new TargetInfo(this.Pawn.Position, this.Pawn.Map, false), null, null);

                    this.Pawn.health.RemoveHediff(this.parent);
                }
            }
            if (!mutate)
            {
                string label = "FailedMutation".Translate();
                string text = "FailedMutationLetter".Translate();
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, new TargetInfo(this.Pawn.Position, this.Pawn.Map, false), null, null);

                this.Pawn.health.RemoveHediff(this.parent);
            }
        }
    }
}
