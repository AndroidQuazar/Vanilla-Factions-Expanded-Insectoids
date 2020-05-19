
using Verse;

namespace VFEI
{
    public class CompProperties_FilthProducer : CompProperties
    {

        public string filthType = "";
        public float rate = 0f;
        public int radius = 0;
        public int ticksToCreateFilth = 600;



        public CompProperties_FilthProducer()
        {

            this.compClass = typeof(CompFilthProducer);
        }
    }
}
