using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEI.Other
{
    class VFEI_Plant_Armillarix : Plant
    {
        public override void TickLong()
        {
            base.TickLong();
            if (this.LifeStage == PlantLifeStage.Mature)
            {
                this.BroadcastCompSignal("armillarixOn");
            }
            else
            {
                this.BroadcastCompSignal("armillarixOff");
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            this.Map.glowGrid.DeRegisterGlower(this.TryGetComp<CompGlower>());
            base.DeSpawn(mode);
        }
    }
}
