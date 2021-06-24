using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFEI
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("kikohi.vfe.insectoid").PatchAll();
        }

        [HarmonyPatch(typeof(BodyPartDef), "GetMaxHealth", MethodType.Normal)]
        internal class GetMaxHealth_Postfix
        {
            [HarmonyPostfix]
            private static void PostFix(BodyPartDef __instance, ref float __result, Pawn pawn)
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff?.Part?.def == __instance && HediffUtility.TryGetComp<Comps.VariableHealthComp.VFEI_HediffComp_HealthModifier>(hediff) != null)
                    {
                        __result += HediffUtility.TryGetComp<Comps.VariableHealthComp.VFEI_HediffComp_HealthModifier>(hediff).Props.healthPointToAdd;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CompGlower), "ReceiveCompSignal", MethodType.Normal)]
        internal class ReceiveCompSignal_Postfix
        {
            [HarmonyPostfix]
            private static void PostFix(string signal, ref CompGlower __instance)
            {
                if (__instance?.parent != null)
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
            }
        }

        [HarmonyPatch(typeof(SettlementDefeatUtility), "IsDefeated", MethodType.Normal)]
        internal class IsDefeated_Postfix
        {
            [HarmonyPostfix]
            private static void PostFix(Map map, Faction faction, ref bool __result)
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
        }

        [HarmonyPatch(typeof(ThoughtWorker_Dark), "CurrentStateInternal", MethodType.Normal)]
        internal class CurrentStateInternal_Postfix
        {
            [HarmonyPostfix]
            private static void PostFix(Pawn p, ref ThoughtState __result)
            {
                if (p.Awake() && p.needs.mood.recentMemory.TicksSinceLastLight > 240 && p.health.hediffSet.HasHediff(VFEI_DefsOf.VFEI_Antenna))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }

        [HarmonyPatch(typeof(Faction), "ShouldHaveLeader", MethodType.Getter)]
        internal class ShouldHaveLeader_Postfix
        {
            [HarmonyPostfix]
            private static void PostFix(ref Faction __instance, ref bool __result)
            {
                if (__instance.def.defName == "VFEI_Insect") __result = false;
            }
        }

        [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting", MethodType.Normal)]
        internal class ThoughtsFromIngesting_Postfix
        {
            [HarmonyPostfix]
            private static void PostFix(Pawn ingester, ref List<FoodUtility.ThoughtFromIngesting> __result)
            {
                if (ingester.health.hediffSet.HasHediff(VFEI_DefsOf.VFEI_VenomGland))
                {
                    __result.Clear();
                }
            }
        }

        [HarmonyPatch(typeof(CompGlower), "PostSpawnSetup", MethodType.Normal)]
        internal class PostSpawnSetup_Prefix
        {
            [HarmonyPrefix]
            private static bool Prefix(ref CompGlower __instance)
            {
                if (__instance.parent.def.defName == "VFEI_Plant_Armillarix")
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_Infestation), "TryExecuteWorker", MethodType.Normal)]
        internal class TryExecuteWorker_Prefix
        {
            [HarmonyPrefix]
            private static bool Prefix(IncidentParms parms, ref bool __result)
            {
                Map map = (Map)parms.target;
                if (map.listerBuildings.AllBuildingsColonistOfDef(DefDatabase<ThingDef>.GetNamed("VFEI_SonicInfestationRepeller")).Count() > 0)
                {
                    if (Find.FactionManager.AllFactionsVisible.Where((f) => f.def.defName == "VFEI_Insect").Count() > 0)
                    {
                        IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                        incidentParms.faction = Find.FactionManager.AllFactionsVisible.Where((f) => f.def.defName == "VFEI_Insect").First();
                        incidentParms.raidStrategy = VFEI_DefsOf.VFEI_ImmediateAttackInsect;
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
        }

        [HarmonyPatch(typeof(GenStep_Settlement), "ScatterAt", MethodType.Normal)]
        internal class ScatterAt_Prefix
        {
            [HarmonyPrefix]
            private static bool Prefix(IntVec3 c, Map map, GenStepParams parms)
            {
                try
                {
                    if (map.ParentFaction?.def.defName == "VFEI_Insect")
                    {
                        IntRange SettlementSizeRange = new IntRange(44, 60);
                        CellRect rect = new CellRect(c.x - SettlementSizeRange.RandomInRange / 2, c.z - SettlementSizeRange.RandomInRange / 2, SettlementSizeRange.RandomInRange, SettlementSizeRange.RandomInRange);
                        rect.ClipInsideMap(map);

                        ResolveParams resolveParams = default;
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
                    Log.Message(ex.Message);
                    return true;
                }
            }
        }
    }
}