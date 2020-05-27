﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace VFEI
{
    class CompTargetable_ThingInRadius : CompTargetable
    {
        protected override bool PlayerChoosesTarget
        {
            get
            {
                return false;
            }
        }

        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                validator = ((TargetInfo x) => base.BaseTargetValidator(x.Thing))
            };
        }

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            if (this.parent.MapHeld == null)
            {
                yield break;
            }
            TargetingParameters tp = this.GetTargetingParameters();

            IEnumerable<IntVec3> list = GenRadial.RadialCellsAround(base.parent.Position, 5, true);
            foreach (IntVec3 t in list)
            {
                IEnumerable<Thing> things = t.GetThingList(this.parent.Map);
                foreach (Thing item in things.ToList())
                {
                    if (item.def.defName != "VFEI_Artifacts_ArchotechEraser")
                    {
                        item.Destroy(DestroyMode.Vanish);
                    }
                }
                if (t.Roofed(this.parent.Map))
                {
                    this.parent.Map.roofGrid.SetRoof(t, null);
                }
            }
            yield break;
        }
    }
}
 