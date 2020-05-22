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

namespace VFEI
{

    public class HarmonyPatches : Mod
    {
        public HarmonyPatches(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("kikohi.vfe.insectoid");
            harmony.Patch(AccessTools.Method(typeof(SettlementDefeatUtility), "IsDefeated", null, null), null, new HarmonyMethod(typeof(HarmonyPatches), "DefeatedPostfix", null), null, null);
            harmony.Patch(AccessTools.Method(typeof(FoodUtility), "ThoughtsFromIngesting", null, null), null, new HarmonyMethod(typeof(HarmonyPatches), "ThoughtsFromIngestingPrefix", null), null, null);
            // ========== Prefix ========== 
            harmony.Patch(AccessTools.Method(typeof(GenStep_Settlement), "ScatterAt", null, null), new HarmonyMethod(typeof(HarmonyPatches), "InsectoidSettlementGen_Prefix", null), null, null, null);
            Log.Message("VFEI - Harmony patches applied");
        }

        static bool InsectoidSettlementGen_Prefix(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            try
            {
                if (map.ParentFaction != null && map.ParentFaction.def.defName == "VFEI_Insect")
                {
                    IntRange SettlementSizeRange = new IntRange(34, 44);
                    int randomInRange = SettlementSizeRange.RandomInRange;
                    int randomInRange2 = SettlementSizeRange.RandomInRange;
                    CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
                    rect.ClipInsideMap(map);
                    ResolveParams resolveParams = default(ResolveParams);
                    resolveParams.rect = rect;
                    resolveParams.faction = map.ParentFaction;
                    BaseGen.globalSettings.map = map;
                    BaseGen.globalSettings.minBuildings = 1;
                    BaseGen.globalSettings.minBarracks = 1;
                    BaseGen.symbolStack.Push("insectoidBase", resolveParams, null);
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

        static void DefeatedPostfix(Map map, Faction faction, ref bool __result)
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

        static void ThoughtsFromIngestingPrefix(Pawn ingester, Thing foodSource, ThingDef foodDef, ref List<ThoughtDef> __result)
        {
            if (ingester.health.hediffSet.HasHediff(ThingDefsVFEI.VFEI_VenomGland))
            {
                __result.Clear();
            }
        }
    }
}
