using System;
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

            IEnumerable<Thing> list = GenRadial.RadialCellsAround(base.parent.Position, 5, true).SelectMany(c => c.GetThingList(base.parent.Map));
            if (list.Any())
            {
                foreach (Thing t in list.ToList())
                {
                    if(t.def.defName != "VFEI_Artifacts_ArchotechEraser")
                    {
                        t.Destroy(DestroyMode.Vanish);
                    }
                    IntVec3 roofPos = t.Position;
                    if (roofPos.Roofed(this.parent.Map))
                    {
                        this.parent.Map.roofGrid.SetRoof(roofPos, null);
                    }
                }
            }
            yield break;
        }
    }
}
 