using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;

namespace VFEI.Comps.VariableHealthComp
{
    class VFEI_HediffCompProperties_HealthModifier : HediffCompProperties
    {
		public VFEI_HediffCompProperties_HealthModifier()
		{
			this.compClass = typeof(VFEI_HediffComp_HealthModifier);
		}

		public float healthPointToAdd = 0f;
	}
}
