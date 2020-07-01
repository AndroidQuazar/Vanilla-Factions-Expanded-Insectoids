using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace VFEI
{
    class CompSpawnerInsectOnDamaged : ThingComp
    {
        public CompProperties_SpawnerInsectOnDamaged Props
        {
            get
            {
                return (CompProperties_SpawnerInsectOnDamaged)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look<Lord>(ref this.lord, "expandLord", false);
            Scribe_Values.Look<float>(ref this.pointsLeft, "insectPointsLeft", 0f, false);
            Scribe_Values.Look<bool>(ref spawned, "spawned", false);
        }

        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
            if (absorbed)
            {
                return;
            }
            if (dinfo.Def.harmsHealth)
            {
                if (this.lord != null)
                {
                    this.lord.ReceiveMemo(CompSpawnerInsectOnDamaged.MemoDamaged);
                }
                float num = (float)this.parent.HitPoints - dinfo.Amount;
                if ((num < (float)this.parent.MaxHitPoints * 0.98f && dinfo.Instigator != null && dinfo.Instigator.Faction != null) || num < (float)this.parent.MaxHitPoints * 0.9f)
                {
                    List<Thing> otherChunks = this.parent.Map.listerThings.AllThings.Where(thing => thing.def.defName == "VFEI_InfestedCrashedShipModule" || thing.def.defName == "VFEI_InfestedCrashedShipPart" || thing.def.defName == "VFEI_InfestedShipChunk").ToList();
                    foreach (Thing thing in otherChunks)
                    {
                        thing.TryGetComp<CompSpawnerInsectOnDamaged>().TrySpawnInsect();
                    }
                    this.TrySpawnInsect();
                }
            }
            absorbed = false;
        }

        public void Notify_BlueprintReplacedWithSolidThingNearby(Pawn by)
        {
            if (by.Faction != Faction.OfInsects)
            {
                this.TrySpawnInsect();
            }
        }

        private void TrySpawnInsect()
        {
            pointsLeft = Mathf.Max(StorytellerUtility.DefaultSiteThreatPointsNow() * 0.9f, 300f) * Props.level;

            Faction insectFaction = Find.FactionManager.AllFactions.Where((f) => f != null && f.def.defName == "VFEI_Insect").First();
            if (this.lord == null)
            {
                IntVec3 invalid;
                if (!CellFinder.TryFindRandomCellNear(this.parent.Position, this.parent.Map, 5, (IntVec3 c) => c.Standable(this.parent.Map) && this.parent.Map.reachability.CanReach(c, this.parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)), out invalid, -1))
                {
                    Log.Error("Found no place for insects to defend " + this, false);
                    invalid = IntVec3.Invalid;
                }
                LordJob_InsectDefendChunk lordJob = new LordJob_InsectDefendChunk(this.parent, insectFaction, 50, this.parent.Position);
                this.lord = LordMaker.MakeNewLord(insectFaction, lordJob, this.parent.Map, null);
            }

            if (this.pointsLeft <= 0f || this.spawned)
            {
                return;
            }
            if (!this.parent.Spawned)
            {
                return;
            }
            if (spawned)
            {
                return;
            }
            try
            {
                if (Props.level == 1)
                {
                    spawned = true;
                    int count = Rand.RangeInclusive(0, 1);
                    for (int i = 0; i < count; i++)
                    {
                        IntVec3 intVec3 = new IntVec3();
                        RCellFinder.TryFindRandomCellNearWith(this.parent.Position, (p) => p.Walkable(this.parent.Map) == true, this.parent.Map, out intVec3, 2);
                        GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(ThingDefsVFEI.VFEI_Insectoid_Larvae, insectFaction)), intVec3, this.parent.Map);
                        GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Megaspider, insectFaction)), intVec3, this.parent.Map);
                        this.pointsLeft -= PawnKindDefOf.Megaspider.combatPower;
                        this.pointsLeft -= ThingDefsVFEI.VFEI_Insectoid_Larvae.combatPower;
                    }
                    while (this.pointsLeft > 0f)
                    {
                        PawnKindDef kind;
                        IntVec3 intVec3 = new IntVec3();
                        RCellFinder.TryFindRandomCellNearWith(this.parent.Position, (p) => p.Walkable(this.parent.Map) == true, this.parent.Map, out intVec3, 2);
                        if (! (from def in DefDatabase<PawnKindDef>.AllDefs where (def.race.defName == "Megascarab" || def.race.defName == "Spelopede" || def.race.defName == "Megaspider") && def.combatPower <= this.pointsLeft
                                select def).TryRandomElement(out kind))
                        {
                            break;
                        }
                        Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, insectFaction));
                        GenSpawn.Spawn(pawn, intVec3, this.parent.Map);
                        this.lord.AddPawn(pawn);
                        this.pointsLeft -= pawn.kindDef.combatPower;
                    }
                }
                if (Props.level == 2)
                {
                    spawned = true;
                    int count = Rand.RangeInclusive(1, 3);
                    for (int i = 0; i < count; i++)
                    {
                        IntVec3 intVec3 = new IntVec3();
                        RCellFinder.TryFindRandomCellNearWith(this.parent.Position, (p) => p.Walkable(this.parent.Map) == true, this.parent.Map, out intVec3, 2);
                        GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Megaspider, insectFaction)), intVec3, this.parent.Map);
                        this.pointsLeft -= PawnKindDefOf.Megaspider.combatPower;
                    }
                    int count2 = Rand.RangeInclusive(4, 8);
                    for (int i = 0; i < count2; i++)
                    {
                        IntVec3 intVec3 = new IntVec3();
                        RCellFinder.TryFindRandomCellNearWith(this.parent.Position, (p) => p.Walkable(this.parent.Map) == true, this.parent.Map, out intVec3, 2);
                        GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(ThingDefsVFEI.VFEI_Insectoid_Larvae, insectFaction)), intVec3, this.parent.Map);
                        this.pointsLeft -= ThingDefsVFEI.VFEI_Insectoid_Larvae.combatPower;
                    }
                    while (this.pointsLeft > 0f)
                    {
                        IntVec3 intVec3 = new IntVec3();
                        RCellFinder.TryFindRandomCellNearWith(this.parent.Position, (p) => p.Walkable(this.parent.Map) == true, this.parent.Map, out intVec3, 2);
                        PawnKindDef kind;
                        if (!(from def in DefDatabase<PawnKindDef>.AllDefs
                              where (def.race.defName == "Megascarab" || def.race.defName == "Spelopede" || def.race.defName == "Megaspider") && def.combatPower <= this.pointsLeft
                              select def).TryRandomElement(out kind))
                        {
                            break;
                        }
                        Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, insectFaction));
                        GenSpawn.Spawn(pawn, intVec3, this.parent.Map);
                        this.lord.AddPawn(pawn);
                        this.pointsLeft -= pawn.kindDef.combatPower;
                    }
                }
                if (Props.level == 3)
                {
                    spawned = true;
                    int count = Rand.RangeInclusive(0, 1);
                    for (int i = 0; i < count; i++)
                    {
                        IntVec3 intVec3 = new IntVec3();
                        RCellFinder.TryFindRandomCellNearWith(this.parent.Position, (p) => p.Walkable(this.parent.Map) == true, this.parent.Map, out intVec3, 2);
                        GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(ThingDefsVFEI.VFEI_Insectoid_Gigalocust, insectFaction)), intVec3, this.parent.Map);
                        this.pointsLeft -= ThingDefsVFEI.VFEI_Insectoid_Gigalocust.combatPower;
                    }
                    int count2 = Rand.RangeInclusive(4, 8);
                    for (int i = 0; i < count2; i++)
                    {
                        IntVec3 intVec3 = new IntVec3();
                        RCellFinder.TryFindRandomCellNearWith(this.parent.Position, (p) => p.Walkable(this.parent.Map) == true, this.parent.Map, out intVec3, 2);
                        GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(ThingDefsVFEI.VFEI_Insectoid_Larvae, insectFaction)), intVec3, this.parent.Map);
                        this.pointsLeft -= ThingDefsVFEI.VFEI_Insectoid_Larvae.combatPower;
                    }
                    int count3 = Rand.RangeInclusive(1, 4);
                    if (count3 == 4)
                    {
                        IntVec3 intVec3 = new IntVec3();
                        RCellFinder.TryFindRandomCellNearWith(this.parent.Position, (p) => p.Walkable(this.parent.Map) == true, this.parent.Map, out intVec3, 2);
                        GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(ThingDefsVFEI.VFEI_Insectoid_Queen, insectFaction)), intVec3, this.parent.Map);
                        this.pointsLeft -= ThingDefsVFEI.VFEI_Insectoid_Queen.combatPower;
                    }
                    while (this.pointsLeft > 0f)
                    {
                        IntVec3 intVec3 = new IntVec3();
                        RCellFinder.TryFindRandomCellNearWith(this.parent.Position, (p) => p.Walkable(this.parent.Map) == true, this.parent.Map, out intVec3, 2);
                        PawnKindDef kind;
                        if (!(from def in DefDatabase<PawnKindDef>.AllDefs
                              where (def.race.defName == "Megascarab" || def.race.defName == "Spelopede" || def.race.defName == "Megaspider" || def.race.defName == "VFEI_Insectoid_Megapede") && def.combatPower <= this.pointsLeft
                              select def).TryRandomElement(out kind))
                        {
                            break;
                        }
                        Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, insectFaction));
                        GenSpawn.Spawn(pawn, intVec3, this.parent.Map);
                        this.lord.AddPawn(pawn);
                        this.pointsLeft -= pawn.kindDef.combatPower;
                    }
                }
            }
            finally
            {
                this.pointsLeft = 0f;
            }
        }

        private bool CanSpawnInsectAt(IntVec3 c)
        {
            return c.Walkable(this.parent.Map);
        }

        public bool spawned = false;
        public float pointsLeft;
        private Lord lord;
        public static readonly string MemoDamaged = "ShipPartDamaged";
    }
}
