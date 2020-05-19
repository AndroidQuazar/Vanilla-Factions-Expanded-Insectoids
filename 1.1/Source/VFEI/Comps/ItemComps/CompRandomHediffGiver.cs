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

        private int ticksToMutate;

        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref this.ticksToMutate, "ticksToMutate");
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            this.ticksToMutate = Rand.RangeInclusive(100, 600);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            RecipeDef randRecipe = this.Props.allowedRecipeDefs.RandomElement();
            this.Pawn.health.AddHediff(randRecipe.addsHediff, MedicalRecipesUtility.GetFixedPartsToApplyOn(randRecipe, this.Pawn).RandomElement());

            string label = "MutationOutcome".Translate();
            string text = "MutationOutcomeLetter".Translate(randRecipe.label.Substring(8));
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, new TargetInfo(this.Pawn.Position, this.Pawn.Map, false), null, null);

            this.Pawn.health.RemoveHediff(this.parent);
        }
    }
}
