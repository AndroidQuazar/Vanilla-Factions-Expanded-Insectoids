using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;

namespace VFEI.Comps.VariableHealthComp
{
    class VFEI_HediffComp_HealthModifier : HediffComp
    {
		public VFEI_HediffCompProperties_HealthModifier Props
		{
			get
			{
				return (VFEI_HediffCompProperties_HealthModifier)this.props;
			}
		}
	}
}
