using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using UnityEngine;
using Verse;

namespace VFEI.Comps.ItemComps.Properties
{
    class CompProperties_RandomHediffGiver : HediffCompProperties
    {
        public CompProperties_RandomHediffGiver()
        {
            this.compClass = typeof(CompRandomHediffGiver);
        }

        #pragma warning disable 0649
        public List<RecipeDef> allowedRecipeDefs;
    }
}
