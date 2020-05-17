using Verse;
using System.Collections.Generic;

namespace VFEI
{
    public class CompProperties_DigPeriodically : CompProperties
    {


        public List<string> customThingToDig = null;
        public List<int> customAmountToDig = null;
        public int ticksToDig = 60000;

        public CompProperties_DigPeriodically()
        {
            this.compClass = typeof(CompDigPeriodically);
        }


    }
}