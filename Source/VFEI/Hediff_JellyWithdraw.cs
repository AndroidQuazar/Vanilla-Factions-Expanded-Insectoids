using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace VFEI
{
    class Hediff_JellyWithdraw : Hediff_Addiction
    {

        bool firstTime = true;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref this.firstTime, "firstTime");
        }

        public override void Tick()
        {
            base.Tick();
            Need_Chemical need = this.Need;
            if (this.firstTime && need.CurCategory == DrugDesireCategory.Withdrawal)
            {
                Random random = new Random();
                int flag = random.Next(0, 11);
                if (flag == 5)
                {
                    Faction colonistFac = this.pawn.Faction;
                    IntVec3 colonistLoc = this.pawn.Position;
                    Name colonistName = this.pawn.Name;
                    Map map = this.pawn.Map;
                    this.pawn.Destroy(DestroyMode.Vanish);
                    PawnKindDef pawnKindDef = PawnKindDefOf.Megaspider;
                    Pawn megaspider = PawnGenerator.GeneratePawn(pawnKindDef, colonistFac);
                    megaspider.Name = colonistName;
                    GenSpawn.Spawn(megaspider, colonistLoc, map, WipeMode.Vanish);
                }
                this.firstTime = false;
            }
        }
    }
}
