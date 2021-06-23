using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFEI
{
    internal class CompTargetable_ThingInRadius : CompTargetable
    {
        protected override bool PlayerChoosesTarget
        {
            get
            {
                return false;
            }
        }

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            if (this.parent.MapHeld == null)
            {
                yield break;
            }
            TargetingParameters tp = this.GetTargetingParameters();

            IEnumerable<IntVec3> list = GenRadial.RadialCellsAround(base.parent.Position, 10, true);
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
            FleckMaker.Static(this.parent.TrueCenter(), this.parent.Map, FleckDefOf.PsycastAreaEffect, 10);
            yield break;
        }

        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                validator = ((TargetInfo x) => base.BaseTargetValidator(x.Thing))
            };
        }
    }
}