using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEI.Other
{
    class Recipe_AddMutationHediff : RecipeWorker
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            pawn.health.AddHediff(this.recipe.addsHediff);
        }
    }
}
