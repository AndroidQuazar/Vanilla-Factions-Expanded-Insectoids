using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace VFEI.Other
{
    class MapComponent_VFEI_Utils : MapComponent
    {
        public MapComponent_VFEI_Utils(Map map) : base(map)
        {
        }

        public override void FinalizeInit()
        {
            PlayerKnowledgeDatabase.SetKnowledge(ConceptDefOf.ShieldBelts, 1f);
        }
    }
}
