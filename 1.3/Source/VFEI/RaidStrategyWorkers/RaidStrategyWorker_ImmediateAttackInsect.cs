using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace VFEI.RaidStrategyWorkers
{
    public class RaidStrategyWorker_ImmediateAttackInsect : RaidStrategyWorker
    {
        public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            return base.CanUseWith(parms, groupKind) && parms.faction.def.defName == "VFEI_Insect";
        }

        public override List<Pawn> SpawnThreats(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Building> ts = map.listerBuildings.allBuildingsColonist;
            ts.RemoveAll((b) => b.def.defName != "VFEI_SonicInfestationRepeller");
            int toAdd = Rand.RangeInclusive(0, ts.Count) * 300;
            parms.points += toAdd;
            return base.SpawnThreats(parms);
        }

        protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
        {
            return new LordJob_AssaultColony(parms.faction, false, false, false, false, false);
        }
    }
}