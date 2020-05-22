using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using UnityEngine;
using Verse;

namespace VFEI.Comps.HediffComps
{
    class CompSpawnJelly : HediffComp
    {
        public CompProperties.CompProperties_SpawnJelly Props
        {
            get
            {
                return (CompProperties.CompProperties_SpawnJelly)this.props;
            }
        }

        public int NextBeforeSpawn = 0;

        public override void CompPostMake()
        {
            this.NextBeforeSpawn = Find.TickManager.TicksGame + 60000;
            base.CompPostMake();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Find.TickManager.TicksGame >= this.NextBeforeSpawn)
            {
                this.NextBeforeSpawn += 60000;
                Thing thing = ThingMaker.MakeThing(ThingDefOf.InsectJelly);
                thing.stackCount = Rand.RangeInclusive(2, 8);
                GenSpawn.Spawn(thing, this.Pawn.Position, this.Pawn.Map);
            } 
        }
    }
}
