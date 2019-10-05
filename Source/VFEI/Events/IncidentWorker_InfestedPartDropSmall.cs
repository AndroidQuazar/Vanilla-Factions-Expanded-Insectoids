using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace VFEI
{
    class IncidentWorker_InfestedPartDropSmall : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 intVec;
            return base.CanFireNowSub(parms) && this.TryFindShipChunkDropCell(map.Center, map, 999999, out intVec);
        }

        protected int RandomCountToDrop(IncidentParms parms)
        {
            int num = (int)parms.points / 10;
            int count = 3;
            if (num >= 100)
            {
                count = Rand.RangeInclusive(4, 7);
            }
            if (num >= 200)
            {
                count = Rand.RangeInclusive(7, 10);
            }
            if (num >= 300)
            {
                count = Rand.RangeInclusive(10, 13);
            }
            if (num >= 500)
            {
                count = Rand.RangeInclusive(13, 16);
            }
            if (num >= 900)
            {
                count = Rand.RangeInclusive(16, 28);
            }
            return count;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 intVec;
            bool flag = !this.TryFindShipChunkDropCell(map.Center, map, 999999, out intVec);
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                this.SpawnShipChunks(intVec, map, this.RandomCountToDrop(parms));
                Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterDef, new GlobalTargetInfo(intVec, map, false), null, null);
                result = true;
            }
            return result;
        }

        protected bool TryFindShipChunkDropCell(IntVec3 nearLoc, Map map, int maxDist, out IntVec3 pos)
        {
            ThingDef infestedShipChunkIncoming = ThingDefsVFEI.VFEI_InfestedShipChunkIncoming;
            return CellFinderLoose.TryFindSkyfallerCell(infestedShipChunkIncoming, map, out pos, 40, nearLoc, maxDist, true, false, false, false, true, false, null);
        }

        protected void SpawnChunk(IntVec3 pos, Map map)
        {
            SkyfallerMaker.SpawnSkyfaller(ThingDefsVFEI.VFEI_InfestedShipChunkIncoming, ThingDefsVFEI.VFEI_InfestedShipChunk, pos, map);
        }

        protected void SpawnShipChunks(IntVec3 firstChunkPos, Map map, int count)
        {
            this.SpawnChunk(firstChunkPos, map);
            for (int i = 0; i < count - 1; i++)
            {
                IntVec3 pos;
                bool flag = this.TryFindShipChunkDropCell(firstChunkPos, map, 20, out pos);
                if (flag)
                {
                    this.SpawnChunk(pos, map);
                }
            }
        }

        protected const float HivePoints = 300f;
    }
}
