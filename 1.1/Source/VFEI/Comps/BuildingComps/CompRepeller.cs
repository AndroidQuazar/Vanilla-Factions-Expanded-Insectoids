using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace VFEI.Comps.BuildingComps
{
    class CompRepeller : ThingComp
    {
        public CompProperties_Repeller Props
        {
            get
            {
                return (CompProperties_Repeller)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.nextMote, "nextMote");
        }

        int nextMote = 0;

        public override void PostPostMake()
        {
            this.nextMote = Find.TickManager.TicksGame + 2500;
        }

        public override void CompTickRare()
        {
            if (Find.TickManager.TicksGame >= this.nextMote)
            {
                MoteMaker.MakeStaticMote(this.parent.TrueCenter(), this.parent.Map, ThingDefOf.Mote_PsycastAreaEffect, 10);
                this.nextMote += 2500;
            }
        }
    }
}
