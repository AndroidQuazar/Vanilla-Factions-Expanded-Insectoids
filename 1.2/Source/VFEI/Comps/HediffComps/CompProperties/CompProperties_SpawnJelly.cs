using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using UnityEngine;
using Verse;

namespace VFEI.Comps.HediffComps.CompProperties
{
    class CompProperties_SpawnJelly : HediffCompProperties
    {
        public CompProperties_SpawnJelly()
        {
            this.compClass = typeof(CompSpawnJelly);
        }
    }
}
