using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace VFEI.Other
{
    class MapComp_FacRelationUtils : MapComponent
    {
        public MapComp_FacRelationUtils(Map map) : base(map)
        {
        }

        public override void FinalizeInit()
        {
            Log.Message("VFEI - Relation fixer");
            Faction fac = Find.FactionManager.AllFactions.Where(f => f.def.defName == "VFEI_Insect").First();
            Log.Message(fac.RelationKindWith(Faction.OfInsects).ToString());
            Log.Message(fac.GoodwillWith(Faction.OfInsects).ToString());
        }
    }
}
