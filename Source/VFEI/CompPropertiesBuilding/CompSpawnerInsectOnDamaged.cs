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
                    List<Thing> otherChunks = this.parent.Map.listerThings.AllThings.Where(thing => thing.def.defName == this.parent.def.defName).ToList();
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

        private bool queenSpawned = false;

        private void TrySpawnInsect()
        {
            pointsLeft = Mathf.Max(StorytellerUtility.DefaultSiteThreatPointsNow() * 0.9f, 300f) * Props.level;
            

            if (this.pointsLeft <= 0f || this.spawned)
            {
                return;
            }
            if (!this.parent.Spawned)
            {
                return;
            }
            if (this.lord == null && Props.level != 3)
            {
                IntVec3 invalid;
                if (!CellFinder.TryFindRandomCellNear(this.parent.Position, this.parent.Map, 5, (IntVec3 c) => c.Standable(this.parent.Map) && this.parent.Map.reachability.CanReach(c, this.parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)), out invalid, -1))
                {
                    Log.Error("Found no place for insects to defend " + this, false);
                    invalid = IntVec3.Invalid;
                }
                LordJob_InsectDefendChunk lordJob = new LordJob_InsectDefendChunk(this.parent, Faction.OfInsects, 50, this.parent.Position);
                this.lord = LordMaker.MakeNewLord(Faction.OfInsects, lordJob, this.parent.Map, null);
            }
            if (this.Props.level == 3) //ATTACK UNTIL DEAAD JOB
            {
                IntVec3 invalid;
                if (!CellFinder.TryFindRandomCellNear(this.parent.Position, this.parent.Map, 5, (IntVec3 c) => c.Standable(this.parent.Map) && this.parent.Map.reachability.CanReach(c, this.parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)), out invalid, -1))
                {
                    Log.Error("Found no place for insects to move " + this, false);
                    invalid = IntVec3.Invalid;
                }
                LordJob_InsectAttack lordJob = new LordJob_InsectAttack(Faction.OfInsects);
                this.lord = LordMaker.MakeNewLord(Faction.OfInsects, lordJob, this.parent.Map, null);
            }
            try
            {
                while (this.pointsLeft > 0f)
                {
                    PawnKindDef kind;
                    if (!(from def in DefDatabase<PawnKindDef>.AllDefs
                          where (def.race.defName == "Megascarab" || def.race.defName == "Spelopede" || def.race.defName == "Megaspider") && def.combatPower <= this.pointsLeft
                          select def).TryRandomElement(out kind) && Props.level != 1)
                    {
                        break;
                    }
                    if (!(from def in DefDatabase<PawnKindDef>.AllDefs
                          where (def.race.defName == "Megascarab" || def.race.defName == "Spelopede" || def.race.defName == "Megaspider" || def.race.defName == "VFEI_Insectoid_Beetle") && def.combatPower <= this.pointsLeft
                          select def).TryRandomElement(out kind) && Props.level != 2)
                    {
                        break;
                    }
                    if (!(from def in DefDatabase<PawnKindDef>.AllDefs
                          where (def.race.defName == "Megascarab" || def.race.defName == "Spelopede" || def.race.defName == "Megaspider" || def.race.defName == "VFEI_Insectoid_Beetle" || def.race.defName == "VFEI_Insectoid_RoyalMegaspider") && def.combatPower <= this.pointsLeft
                          select def).TryRandomElement(out kind) && Props.level != 3)
                    {
                        break;
                    }

                    IntVec3 center;
                    if (!(from cell in GenAdj.CellsAdjacent8Way(this.parent)
                          where this.CanSpawnInsectAt(cell)
                          select cell).TryRandomElement(out center))
                    {
                        break;
                    }

                    // QUEEEN!
                    if (Props.level == 3 && !this.queenSpawned)
                    {
                        PawnGenerationRequest requestQ = new PawnGenerationRequest(ThingDefsVFEI.VFEI_Insectoid_Queen, Faction.OfInsects, PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, null, null, null);
                        Pawn pawnQ = PawnGenerator.GeneratePawn(requestQ);
                        if (!GenPlace.TryPlaceThing(pawnQ, center, this.parent.Map, ThingPlaceMode.Near, null, null))
                        {
                            Find.WorldPawns.PassToWorld(pawnQ, PawnDiscardDecideMode.Discard);
                            break;
                        }
                        this.lord.AddPawn(pawnQ);
                        this.pointsLeft -= pawnQ.kindDef.combatPower;
                        this.queenSpawned = true;
                    }

                    
                    PawnGenerationRequest request = new PawnGenerationRequest(kind, Faction.OfInsects, PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, null, null, null);
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    if (!GenPlace.TryPlaceThing(pawn, center, this.parent.Map, ThingPlaceMode.Near, null, null))
                    {
                        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                        break;
                    }
                    this.lord.AddPawn(pawn);
                    this.pointsLeft -= pawn.kindDef.combatPower;
                }
                this.spawned = true;
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

        //private int ticksUntilJellySpawn = 5;
        public bool spawned = false;
        public float pointsLeft;
        private Lord lord;
        public static readonly string MemoDamaged = "ShipPartDamaged";
    }
}
