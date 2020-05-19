using Verse;

namespace VFEI
{
    public class CompProperties_DieAfterPeriod : CompProperties
    {

        public int timeToDieInTicks = 1000;
        public bool effect = false;
        public string effectFilth = "Filth_Blood";

        public CompProperties_DieAfterPeriod()
        {
            this.compClass = typeof(CompDieAfterPeriod);
        }
    }
}
