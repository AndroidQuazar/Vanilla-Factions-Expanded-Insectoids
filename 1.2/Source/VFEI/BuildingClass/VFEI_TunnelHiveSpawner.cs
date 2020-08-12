using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace VFEI.BuildingClass
{
	[StaticConstructorOnStartup]
    class VFEI_TunnelHiveSpawner : ThingWithComps
    {
		private static List<PawnKindDef> spawnablePawnKinds = new List<PawnKindDef>();

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.secondarySpawnTick, "secondarySpawnTick", 0, false);
			Scribe_Values.Look<bool>(ref this.spawnHive, "spawnHive", true, false);
			Scribe_Values.Look<float>(ref this.insectsPoints, "insectsPoints", 0f, false);
			Scribe_Values.Look<bool>(ref this.spawnedByInfestationThingComp, "spawnedByInfestationThingComp", false, false);
			Scribe_Values.Look<Faction>(ref this.fac, "faction");
			Scribe_Collections.Look<ThingDef>(ref filthTypes, "filthTypes", LookMode.Def);
			Scribe_Collections.Look<PawnKindDef>(ref spawnablePawnKinds, "spawnablePawnKinds", LookMode.Def);
		}

		public override void PostMake()
		{
			filthTypes.Clear();
			filthTypes.Add(ThingDefOf.Filth_Dirt);
			filthTypes.Add(ThingDefOf.Filth_Dirt);
			filthTypes.Add(ThingDefOf.Filth_Dirt);
			filthTypes.Add(ThingDefOf.Filth_RubbleRock);

			spawnablePawnKinds.Clear();
			spawnablePawnKinds.Add(PawnKindDefOf.Megascarab);
			spawnablePawnKinds.Add(PawnKindDefOf.Spelopede);
			spawnablePawnKinds.Add(PawnKindDefOf.Megaspider);
			spawnablePawnKinds.Add(ThingDefsVFEI.VFEI_Insectoid_RoyalMegaspider);
			fac = Find.FactionManager.AllFactionsVisible.Where((f) => f.def.defName == "VFEI_Insect").First();
			base.PostMake();
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				this.secondarySpawnTick = Find.TickManager.TicksGame + this.ResultSpawnDelay.RandomInRange.SecondsToTicks();
			}
			this.CreateSustainer();
		}

		private Faction fac;

		public override void Tick()
		{
			if (base.Spawned)
			{
				this.sustainer.Maintain();
				Vector3 vector = base.Position.ToVector3Shifted();
				IntVec3 c;
				if (Rand.MTBEventOccurs(FilthSpawnMTB, 1f, 1.TicksToSeconds()) && CellFinder.TryFindRandomReachableCellNear(base.Position, base.Map, FilthSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c, 999999))
				{
					FilthMaker.TryMakeFilth(c, base.Map, filthTypes.RandomElement<ThingDef>(), 1, FilthSourceFlags.None);
				}
				if (Rand.MTBEventOccurs(DustMoteSpawnMTB, 1f, 1.TicksToSeconds()))
				{
					MoteMaker.ThrowDustPuffThick(new Vector3(vector.x, 0f, vector.z)
					{
						y = AltitudeLayer.MoteOverhead.AltitudeFor()
					}, base.Map, Rand.Range(1.5f, 3f), new Color(1f, 1f, 1f, 2.5f));
				}
				if (this.secondarySpawnTick <= Find.TickManager.TicksGame)
				{
					this.sustainer.End();
					Map map = base.Map;
					IntVec3 position = base.Position;
					this.Destroy(DestroyMode.Vanish);
					if (this.spawnHive)
					{
						if (Rand.RangeInclusive(1, 4) < 3)
						{
							Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Hive, null), position, map, WipeMode.Vanish);
							hive.SetFaction(fac, null);
							hive.questTags = this.questTags;
							foreach (CompSpawner compSpawner in hive.GetComps<CompSpawner>())
							{
								if (compSpawner.PropsSpawner.thingToSpawn == ThingDefOf.InsectJelly)
								{
									compSpawner.TryDoSpawn();
									break;
								}
							}
						}
						else
						{
							Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefsVFEI.VFEI_LargeHive, null), position, map, WipeMode.Vanish);
							hive.SetFaction(fac, null);
							hive.questTags = this.questTags;
							foreach (CompSpawner compSpawner in hive.GetComps<CompSpawner>())
							{
								if (compSpawner.PropsSpawner.thingToSpawn == ThingDefsVFEI.VFEI_RoyalInsectJelly)
								{
									compSpawner.TryDoSpawn();
									break;
								}
							}
						}
						if (map.mapPawns.AllPawnsSpawned.Where((p)=>p.kindDef.defName== "VFEI_Insectoid_Queen").Count() < 1)
						{
							List<Pawn> list = new List<Pawn>();
							Pawn queen = PawnGenerator.GeneratePawn(ThingDefsVFEI.VFEI_Insectoid_Queen, fac);
							GenSpawn.Spawn(queen, CellFinder.RandomClosewalkCellNear(position, map, 2, null), map, WipeMode.Vanish);
							queen.mindState.spawnedByInfestationThingComp = this.spawnedByInfestationThingComp;
							list.Add(queen);
							if (list.Any<Pawn>())
							{
								SpawnedPawnParams spp = new SpawnedPawnParams();
								spp.aggressive = false;
								spp.defSpot = position;
								spp.defendRadius = 5;
								LordMaker.MakeNewLord(fac, new LordJob_DefendAndExpandHive(spp), map, list);
							}
						}
					}
					if (this.insectsPoints > 0f)
					{
						this.insectsPoints = Mathf.Max(this.insectsPoints, spawnablePawnKinds.Min((PawnKindDef x) => x.combatPower));
						float pointsLeft = this.insectsPoints;
						List<Pawn> list = new List<Pawn>();
						int num = 0;
						while (pointsLeft > 0f)
						{
							num++;
							if (num > 1000)
							{
								Log.Error("Too many iterations.", false);
								break;
							}
							IEnumerable<PawnKindDef> spawnablePawnKinds = VFEI_TunnelHiveSpawner.spawnablePawnKinds;
							PawnKindDef pawnKindDef;
							if (!spawnablePawnKinds.Where((PawnKindDef x) => x.combatPower <= pointsLeft).TryRandomElement(out pawnKindDef)) break;
							Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, Faction.OfInsects);
							GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(position, map, 2, null), map, WipeMode.Vanish);
							pawn.mindState.spawnedByInfestationThingComp = this.spawnedByInfestationThingComp;
							list.Add(pawn);
							pointsLeft -= pawnKindDef.combatPower;
						}
						if (list.Any<Pawn>())
						{
							LordMaker.MakeNewLord(fac, new LordJob_AssaultColony(fac, true, false, false, false, true), map, list);
						}
					}
				}
			}
		}

		public override void Draw()
		{
			Rand.PushState();
			Rand.Seed = this.thingIDNumber;
			for (int i = 0; i < 6; i++)
			{
				this.DrawDustPart(Rand.Range(0f, 360f), Rand.Range(0.9f, 1.1f) * (float)Rand.Sign * 4f, Rand.Range(1f, 1.5f));
			}
			Rand.PopState();
		}

		private void DrawDustPart(float initialAngle, float speedMultiplier, float scale)
		{
			float num = (Find.TickManager.TicksGame - this.secondarySpawnTick).TicksToSeconds();
			Vector3 pos = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Filth);
			pos.y += 0.0454545468f * Rand.Range(0f, 1f);
			Color value = new Color(0.470588237f, 0.384313732f, 0.3254902f, 0.7f);
			matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
			Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0f, initialAngle + speedMultiplier * num, 0f), Vector3.one * scale);
			Graphics.DrawMesh(MeshPool.plane10, matrix, TunnelMaterial, 0, null, 0, matPropertyBlock);
		}

		private void CreateSustainer()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				SoundDef tunnel = SoundDefOf.Tunnel;
				this.sustainer = tunnel.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
			});
		}

		private int secondarySpawnTick;
		public bool spawnHive = true;
		public float insectsPoints;
		public bool spawnedByInfestationThingComp;
		private Sustainer sustainer;
		private static MaterialPropertyBlock matPropertyBlock = new MaterialPropertyBlock();
		private readonly FloatRange ResultSpawnDelay = new FloatRange(26f, 30f);
		[TweakValue("Gameplay", 0f, 1f)]
		private static float DustMoteSpawnMTB = 0.2f;
		[TweakValue("Gameplay", 0f, 1f)]
		private static float FilthSpawnMTB = 0.3f;
		[TweakValue("Gameplay", 0f, 10f)]
		private static float FilthSpawnRadius = 3f;
		private static readonly Material TunnelMaterial = MaterialPool.MatFrom("Things/Filth/Grainy/GrainyA", ShaderDatabase.Transparent);
		private static List<ThingDef> filthTypes = new List<ThingDef>();
	}
}
