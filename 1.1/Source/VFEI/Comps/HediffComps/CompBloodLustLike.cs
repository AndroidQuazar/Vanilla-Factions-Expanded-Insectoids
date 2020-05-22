using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using UnityEngine;
using Verse;

namespace VFEI.Comps.HediffComps
{
    class CompBloodLustLike : HediffComp
    {
        public CompProperties.CompProperties_BloodLustLike Props
        {
            get
            {
                return (CompProperties.CompProperties_BloodLustLike)this.props;
            }
        }
    }
}
