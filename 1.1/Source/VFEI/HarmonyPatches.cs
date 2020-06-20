using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace VFEI
{

    public class HarmonyPatches : Mod
    {
        public HarmonyPatches(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("kikohi.vfe.insectoid");
            /* ========== Postfix ========== */
            harmony.Patch(AccessTools.Method(typeof(SettlementDefeatUtility), "IsDefeated", null, null), null, new HarmonyMethod(typeof(HarmonyPatches), "Defeated_Postfix", null), null, null);
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Dark), "CurrentStateInternal", null, null), null, new HarmonyMethod(typeof(HarmonyPatches), "ThoughtWorker_Dark_PostFix", null), null, null);
            // harmony.Patch(AccessTools.Method(typeof(LovePartnerRelationUtility), "LovePartnerRelationGenerationChance", null, null), null, new HarmonyMethod(typeof(HarmonyPatches), "LovePartnerRelationGenerationChance_Postfix", null), null, null);
            harmony.Patch(AccessTools.Method(typeof(CompGlower), "ReceiveCompSignal", null, null), null, new HarmonyMethod(typeof(HarmonyPatches), "ReceiveCompSignal_PostFix", null), null, null);
            harmony.Patch(AccessTools.Method(typeof(CompTransporter), "CompGetGizmosExtra", null, null), null, new HarmonyMethod(typeof(HarmonyPatches), "CompGetGizmosExtra_Fix", null), null, null);
            /* ========== Prefix ========== */
            harmony.Patch(AccessTools.Method(typeof(IncidentWorker_Infestation), "TryExecuteWorker", null, null), new HarmonyMethod(typeof(HarmonyPatches), "IncidentWorker_Infestation_Prefix", null), null, null, null);
            harmony.Patch(AccessTools.Method(typeof(GenStep_Settlement), "ScatterAt", null, null), new HarmonyMethod(typeof(HarmonyPatches), "InsectoidSettlementGen_Prefix", null), null, null, null);
            harmony.Patch(AccessTools.Method(typeof(Faction), "TryMakeInitialRelationsWith", null, null), new HarmonyMethod(typeof(HarmonyPatches), "Faction_TryMakeInitialRelationsWith_Prefix", null), null, null, null);
            harmony.Patch(AccessTools.Method(typeof(CompGlower), "PostSpawnSetup", null, null), new HarmonyMethod(typeof(HarmonyPatches), "PostSpawnSetup_PreFix", null), null, null, null);
            harmony.Patch(AccessTools.Method(typeof(Command_LoadToTransporter), "ProcessInput", null, null), new HarmonyMethod(typeof(HarmonyPatches), "ProcessInput_PreFix", null), null, null, null);
            // harmony.Patch(AccessTools.Method(typeof(TransportPodsArrivalAction_LandInSpecificCell), "Arrived", null, null), new HarmonyMethod(typeof(HarmonyPatches), "Arrived_Fix", null), null, null, null);
            Log.Message("VFEI - Harmony patches applied");
        }

        static bool Arrived_Fix(List<ActiveDropPodInfo> pods, int tile, ref TransportPodsArrivalAction_LandInSpecificCell __instance, ref MapParent ___mapParent, ref IntVec3 ___cell)
        {
            /*if (___mapParent.def.defName == "VFEI_TTraveling")
            {
                Log.Message("lalao");
                Thing lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
                foreach (ActiveDropPodInfo podInfo in pods)
                {
                    for (int i = podInfo.innerContainer.Count - 1; i >= 0; i--)
                    {
                        GenPlace.TryPlaceThing(podInfo.innerContainer[i], ___cell, ___mapParent.Map, ThingPlaceMode.Near, delegate (Thing thing, int count)
                        {
                            PawnUtility.RecoverFromUnwalkablePositionOrKill(thing.Position, thing.Map);
                        }, null, podInfo.innerContainer[i].def.defaultPlacingRot);
                    }
                }
                // TransportPodsArrivalActionUtility.DropTravelingTransportPods(pods, this.cell, this.mapParent.Map);
                Messages.Message("TeleportDone".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion, true);
                return false;
            }*/
            return true;
        }

        static void CompGetGizmosExtra_Fix(ref CompTransporter __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance.parent.def.defName == "VFEI_Artifacts_ArchotechTeleporter")
            {
                List<Gizmo> lr = __result.ToList();
                lr.FindAll((g) => g.disabledReason == "CommandLoadTransporterFailNotConnectedToFuelingPort".Translate() || g.disabledReason == "CommandLoadTransporterFailNoFuel".Translate()).ForEach((g) => g.disabled = false);
                __result = lr;
            }
        }

        static bool ProcessInput_PreFix(Event ev, ref Command_LoadToTransporter __instance, ref List<CompTransporter> ___transporters)
        {
            if (___transporters == null)
            {
                ___transporters = new List<CompTransporter>();
            }
            if (!___transporters.Contains(__instance.transComp))
            {
                ___transporters.Add(__instance.transComp);
            }
            for (int j = 0; j < ___transporters.Count; j++)
            {
                if (___transporters[j] != __instance.transComp && !__instance.transComp.Map.reachability.CanReach(__instance.transComp.parent.Position, ___transporters[j].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
                {
                    Messages.Message("MessageTransporterUnreachable".Translate(), ___transporters[j].parent, MessageTypeDefOf.RejectInput, false);
                    return false;
                }
            }
            if (___transporters[0].parent.def.defName == "VFEI_Artifacts_ArchotechTeleporter")
            {
                Find.WindowStack.Add(new Other.Dialog_LoadTeleporter(__instance.transComp.Map, ___transporters));
                return false;
            }
            ___transporters.Clear();
            return true;
        }

        static bool PostSpawnSetup_PreFix(ref CompGlower __instance)
        {
            if (__instance.parent.def.defName == "VFEI_Plant_Armillarix")
            {
                return false;
            }
            return true;
        }

        static void ReceiveCompSignal_PostFix(string signal, ref CompGlower __instance)
        {
            Map map = __instance.parent.Map;
            if (signal == "armillarixOn")
            {
                map.mapDrawer.MapMeshDirty(__instance.parent.Position, MapMeshFlag.Things);
                map.glowGrid.RegisterGlower(__instance);
            }
            else if (signal == "armillarixOff")
            {
                map.mapDrawer.MapMeshDirty(__instance.parent.Position, MapMeshFlag.Things);
                map.glowGrid.DeRegisterGlower(__instance);
            }
        }

        static bool Faction_TryMakeInitialRelationsWith_Prefix(Faction other, Faction __instance, ref List<FactionRelation> ___relations)
        {
            if (other.def.defName == "Insect" && __instance.def.defName == "VFEI_Insect")
            {
                FactionRelation factionRelation = new FactionRelation();
                factionRelation.other = other;
                factionRelation.goodwill = 100;
                factionRelation.kind = FactionRelationKind.Ally;
                ___relations.Add(factionRelation);
                other.TryMakeInitialRelationsWith(__instance);
                return false;
            }
            else if (other.def.defName == "VFEI_Insect" && __instance.def.defName == "Insect")
            {
                FactionRelation factionRelation = new FactionRelation();
                factionRelation.other = other;
                factionRelation.goodwill = 100;
                factionRelation.kind = FactionRelationKind.Ally;
                ___relations.Add(factionRelation);
                return false;
            }
            return true;
        }

        static void LovePartnerRelationGenerationChance_Postfix(ref float __result)
        {
            if (Find.Storyteller.def.defName == "VFEI_Empress")
            {
                __result = 0f;
            }
        }

        static bool IncidentWorker_Infestation_Prefix(IncidentParms parms, ref bool __result)
        {
            Map map1 = (Map)parms.target;
            if (map1.listerBuildings.AllBuildingsColonistOfDef(DefDatabase<ThingDef>.GetNamed("VFEI_SonicInfestationRepeller")).Count() > 0)
            {
                if (Find.FactionManager.AllFactionsVisible.Where((f) => f.def.defName == "VFEI_Insect").Count() > 0)
                {
                    IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map1);
                    incidentParms.faction = Find.FactionManager.AllFactionsVisible.Where((f) => f.def.defName == "VFEI_Insect").First();
                    incidentParms.raidStrategy = ThingDefsVFEI.VFEI_ImmediateAttackInsect;
                    incidentParms.points = (float)1.5 * incidentParms.points;
                    Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame, incidentParms);
                }
                Messages.Message("InfestationNegated".Translate(), MessageTypeDefOf.NeutralEvent);
                __result = true;
                return false;
            }
            else
            {
                __result = true;
                return true;
            }
        }

        static void ThoughtWorker_Dark_PostFix(Pawn p, ref ThoughtState __result)
        {
            if (p.Awake() && p.needs.mood.recentMemory.TicksSinceLastLight > 240 && p.health.hediffSet.HasHediff(ThingDefsVFEI.VFEI_Antenna))
            {
                __result = ThoughtState.Inactive;
            }
        }

        static bool InsectoidSettlementGen_Prefix(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            try
            {
                if (map.ParentFaction != null && map.ParentFaction.def.defName == "VFEI_Insect")
                {
                    IntRange SettlementSizeRange = new IntRange(44, 60);
                    int randomInRange = SettlementSizeRange.RandomInRange;
                    int randomInRange2 = SettlementSizeRange.RandomInRange;
                    CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
                    rect.ClipInsideMap(map);
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.rect = rect;
                    resolveParams.faction = map.ParentFaction;
                    resolveParams.cultivatedPlantDef = ThingDefOf.Plant_Grass;
                    resolveParams.pathwayFloorDef = DefDatabase<TerrainDef>.AllDefsListForReading.FindAll(t => t.terrainAffordanceNeeded == TerrainAffordanceDefOf.Medium && t.costStuffCount < 6).RandomElement();
                    resolveParams.wallStuff = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.stuffProps != null && (t.terrainAffordanceNeeded == TerrainAffordanceDefOf.Light || t.terrainAffordanceNeeded == TerrainAffordanceDefOf.Medium || t.terrainAffordanceNeeded == TerrainAffordanceDefOf.Heavy) && t.BaseMarketValue < 6).RandomElement();
                    BaseGen.globalSettings.map = map;
                    BaseGen.globalSettings.minBuildings = 8;
                    BaseGen.globalSettings.minBarracks = 2;
                    BaseGen.symbolStack.Push("insectoidBase", resolveParams, null);
                    BaseGen.Generate();
                    BaseGen.globalSettings.map = map;
                    BaseGen.symbolStack.Push("insectoidBaseRDamage", resolveParams, null);
                    BaseGen.Generate();
                    BaseGen.globalSettings.map = map;
                    BaseGen.symbolStack.Push("insectoidRandHives", resolveParams, null);
                    BaseGen.Generate();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Message(ex.Message, false);
                return true;
            }
        }

        static void Defeated_Postfix(Map map, Faction faction, ref bool __result)
        {
            List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
            for (int i = 0; i < list.Count; i++)
            {
                Pawn pawn = list[i];
                if (faction.def.defName == "VFEI_Insect" && pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid && GenHostility.IsActiveThreatToPlayer(pawn))
                {
                    __result = false;
                }
            }
        }

        static void ThoughtsFromIngesting_PostFix(Pawn ingester, Thing foodSource, ThingDef foodDef, ref List<ThoughtDef> __result)
        {
            if (ingester.health.hediffSet.HasHediff(ThingDefsVFEI.VFEI_VenomGland))
            {
                __result.Clear();
            }
        }
    }
}
