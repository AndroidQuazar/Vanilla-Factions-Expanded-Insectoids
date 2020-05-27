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

namespace VFEI.Other
{
    class InfestationUtils_MapComponent : MapComponent
    {
        public InfestationUtils_MapComponent(Map map) : base(map)
        {
        }

        public List<IntVec3> intVec3s = null;
        bool once = true;

        public void AllCellCoveredByRepeller(Map map)
        {
            List<Building> buildings = map.listerBuildings.allBuildingsColonist;
            buildings.RemoveAll((b) => b.def.defName != "VFEI_SonicInfestationRepeller");
            foreach (Building item in buildings)
            {
                IEnumerable<IntVec3> affectedCells = GenRadial.RadialCellsAround(item.TrueCenter().ToIntVec3(), 50, true);
                foreach (IntVec3 intVec in affectedCells)
                {
                    intVec3s.Add(intVec);
                }
            }
        }

        public override void MapComponentTick()
        {
            Log.Message("t");
            if (once)
            {
                once = false;
                AllCellCoveredByRepeller(map);
                Log.Message("Inside MapComp tick");
            }
        }
    }
}
