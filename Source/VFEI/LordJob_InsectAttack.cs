using System;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace VFEI
{
    class LordJob_InsectAttack : LordJob
    {
        private Faction faction;

        public LordJob_InsectAttack()
        {
        }

        public LordJob_InsectAttack(Faction faction)
        {
            this.faction = faction;
        }

        public override bool CanBlockHostileVisitors
        {
            get
            {
                return false;
            }
        }

        public override bool AddFleeToil
        {
            get
            {
                return false;
            }
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();

            // Start by assault
            LordToil_AssaultColony lordToil_AssaultColony1 = new LordToil_AssaultColony(false);
            stateGraph.StartingToil = lordToil_AssaultColony1;

            return stateGraph;
        }

        public override void ExposeData()
        {
            Scribe_References.Look<Faction>(ref this.faction, "faction", false);
        }
    }
}
