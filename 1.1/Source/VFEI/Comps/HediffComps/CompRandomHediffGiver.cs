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

        bool ApplyMutation(RecipeDef recipeDef, out BodyPartRecord bodyPartRecord, out bool isImplant)
        {
            isImplant = false;
            bodyPartRecord = MedicalRecipesUtility.GetFixedPartsToApplyOn(recipeDef, this.Pawn).RandomElement();
            if (recipeDef.addsHediff.addedPartProps == null || recipeDef.addsHediff == ThingDefsVFEI.VFEI_SynapticCerebellum) // IsImplant?
            {
                // Log.Message("ApplyMutation: Implant");
                if (!this.Pawn.health.hediffSet.HasHediff(recipeDef.addsHediff)) // If don't already have it
                {
                    // Log.Message("ApplyMutation: Don't have it -> Proceed");
                    isImplant = true;
                    return true; // Mutate
                }
                // Log.Message("ApplyMutation: Has it -> Exit");
                return false; // Don't mutate
            }
            else // Not an implant
            {
                // Log.Message("ApplyMutation: Not implant");
                List<Hediff> hediffs = this.Pawn.health.hediffSet.hediffs;
                for (int i = hediffs.Count - 1; i >= 0; i--) // Check each part for already existing part
                {
                    if (hediffs[i].Part == bodyPartRecord && !hediffs[i].def.tendable) // If part have heddiff, and it's not tentable
                    {
                        // Log.Message("ApplyMutation: Has part at wanted place -> Exit");
                        return false; // Don't mutate
                    }
                }
                // Log.Message("ApplyMutation: No part at wanted place -> Proceed");
                return true; // Mutate
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            int MaxTryNumb = 20;
            bool mutate = false;
            for (int i = 0; i < MaxTryNumb; i++)
            {
                RecipeDef randRecipe = this.Props.allowedRecipeDefs.RandomElement();
                BodyPartRecord bodyPartRecord;
                bool isImplant;
                mutate = ApplyMutation(randRecipe, out bodyPartRecord, out isImplant);
                if (mutate)
                {
                    // Log.Message("Mutate: " + randRecipe.defName);
                    if (isImplant || randRecipe.defName == "VFEI_InstallSynapticCerebellum")
                    {
                        // Log.Message("Implant: " + randRecipe.defName);
                        MaxTryNumb = i;
                        this.Pawn.health.AddHediff(randRecipe.addsHediff, bodyPartRecord);

                        string label1 = "MutationOutcome".Translate();
                        string text1 = "MutationOutcomeLetter".Translate(randRecipe.label.Substring(8));
                        Find.LetterStack.ReceiveLetter(label1, text1, LetterDefOf.NeutralEvent, new TargetInfo(this.Pawn.Position, this.Pawn.Map, false), null, null);

                        this.Pawn.health.RemoveHediff(this.parent);
                    }
                    else if (!isImplant)
                    {
                        // Log.Message("Not an implant: " + randRecipe.defName);
                        MaxTryNumb = i;
                        this.Pawn.health.RestorePart(bodyPartRecord);
                        this.Pawn.health.AddHediff(randRecipe.addsHediff, bodyPartRecord);

                        string label1 = "MutationOutcome".Translate();
                        string text1 = "MutationOutcomeLetter".Translate(randRecipe.label.Substring(8));
                        Find.LetterStack.ReceiveLetter(label1, text1, LetterDefOf.NeutralEvent, new TargetInfo(this.Pawn.Position, this.Pawn.Map, false), null, null);

                        this.Pawn.health.RemoveHediff(this.parent);
                    }
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
