using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEI.Other
{
    class RepellerPlaceWorker : PlaceWorker
    {
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			GenDraw.DrawRadiusRing(loc, 50);
			return true;
		}
	}
}
