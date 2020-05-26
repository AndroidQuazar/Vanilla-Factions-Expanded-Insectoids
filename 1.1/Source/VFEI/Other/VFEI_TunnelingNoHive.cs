using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace VFEI
{
    [StaticConstructorOnStartup]
    class VFEI_TunnelingNoHive : ThingWithComps
    {
		public void ResetStaticData()
		{
			filthTypes.Clear();
			filthTypes.Add(ThingDefOf.Filth_Dirt);
			filthTypes.Add(ThingDefOf.Filth_Dirt);
			filthTypes.Add(ThingDefOf.Filth_Dirt);
			filthTypes.Add(ThingDefOf.Filth_RubbleRock);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.secondarySpawnTick, "secondarySpawnTick");
			Scribe_Collections.Look<ThingDef>(ref this.filthTypes, "filthTypes");
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				this.secondarySpawnTick = Find.TickManager.TicksGame + 200;
			}
			this.ResetStaticData();
		}

		public override void Tick()
		{
			if (base.Spawned)
			{
				this.ResetStaticData();
				Vector3 vector = base.Position.ToVector3Shifted();
				IntVec3 c;
				if (CellFinder.TryFindRandomReachableCellNear(base.Position, base.Map, FilthSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c, 999999))
				{
					FilthMaker.TryMakeFilth(c, base.Map, filthTypes.RandomElement<ThingDef>(), 1, FilthSourceFlags.None);
				}
				MoteMaker.ThrowDustPuffThick(new Vector3(vector.x, 0f, vector.z)
				{
					y = AltitudeLayer.MoteOverhead.AltitudeFor()
				}, base.Map, Rand.Range(1.5f, 3f), new Color(1f, 1f, 1f, 2.5f));

				if (this.secondarySpawnTick <= Find.TickManager.TicksGame)
				{
					this.Destroy(DestroyMode.Vanish);
				}
			}
		}

		private int secondarySpawnTick;
		private readonly FloatRange ResultSpawnDelay = new FloatRange(26f, 30f);
		private static float FilthSpawnRadius = 3f;
		public List<ThingDef> filthTypes = new List<ThingDef>();
	}
}
