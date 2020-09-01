
using RimWorld;
using Verse;


namespace VFEI
{
    class CompFilthProducer : ThingComp
    {

        private int gasProgress = 0;
      
        private System.Random rand = new System.Random();


        public CompProperties_FilthProducer Props
        {
            get
            {
                return (CompProperties_FilthProducer)this.props;
            }
        }
        public override void CompTick()
        {


            gasProgress++;
            if (this.gasProgress > Props.ticksToCreateFilth)
            {
                Pawn pawn = this.parent as Pawn;
                if (pawn.Map != null && pawn.Awake() && !pawn.Downed && !pawn.Dead)
                {
                    CellRect rect = GenAdj.OccupiedRect(pawn.Position, pawn.Rotation, IntVec2.One);
                    rect = rect.ExpandedBy(Props.radius);

                    foreach (IntVec3 current in rect.Cells)
                    {
                        if (current.InBounds(pawn.Map) && rand.NextDouble() < Props.rate)
                        {
                            Thing thing = ThingMaker.MakeThing(ThingDef.Named(Props.filthType), null);
                            thing.Rotation = Rot4.North;
                            thing.Position = current;
                            thing.SpawnSetup(pawn.Map, false);
                           
                        }

                    }
                    
                    
                }
                gasProgress = 0;
            }



        }
    }
}