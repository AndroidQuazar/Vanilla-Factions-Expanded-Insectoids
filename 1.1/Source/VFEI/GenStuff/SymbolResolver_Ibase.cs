﻿using System;
using Verse;
using Verse.AI.Group;
using RimWorld.BaseGen;
using RimWorld;
using RimWorld.Planet;

namespace VFEI.GenStuff
{
    class SymbolResolver_Ibase : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
			int num = 0;
			if (rp.edgeDefenseWidth != null)
			{
				num = rp.edgeDefenseWidth.Value;
			}
			else if (rp.rect.Width >= 20 && rp.rect.Height >= 20 && (faction.def.techLevel >= TechLevel.Industrial || Rand.Bool))
			{
				num = (Rand.Bool ? 2 : 4);
			}
			float num2 = (float)rp.rect.Area / 144f * 0.17f;
			BaseGen.globalSettings.minEmptyNodes = ((num2 < 1f) ? 0 : GenMath.RoundRandom(num2));
			Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map, null);
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
			ResolveParams resolveParams = rp;
			resolveParams.rect = rp.rect;
			resolveParams.faction = faction;
			resolveParams.singlePawnLord = singlePawnLord;
			resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement);
			resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms)));
			if (resolveParams.pawnGroupMakerParams == null)
			{
				resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
				resolveParams.pawnGroupMakerParams.tile = map.Tile;
				resolveParams.pawnGroupMakerParams.faction = faction;
				resolveParams.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange);
				resolveParams.pawnGroupMakerParams.inhabitants = true;
				resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
			}
			BaseGen.symbolStack.Push("pawnGroup", resolveParams, null);
			BaseGen.symbolStack.Push("insectoidBaseLightning", rp, null);
			/*if (num > 0)
			{
				ResolveParams resolveParams3 = rp;
				resolveParams3.faction = faction;
				resolveParams3.edgeDefenseWidth = new int?(num);
				resolveParams3.edgeThingMustReachMapEdge = new bool?(rp.edgeThingMustReachMapEdge ?? true);
				BaseGen.symbolStack.Push("insectoidBaseEdgeDefense", resolveParams3, null);
			}*/
			ResolveParams resolveParams4 = rp;
			resolveParams4.rect = rp.rect.ContractedBy(num);
			resolveParams4.faction = faction;
			BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams4, null);
			/*ResolveParams resolveParams5 = rp;
			resolveParams5.rect = rp.rect.ContractedBy(num);
			resolveParams5.faction = faction;
			resolveParams5.floorOnlyIfTerrainSupports = new bool?(rp.floorOnlyIfTerrainSupports ?? true);
			BaseGen.symbolStack.Push("basePart_outdoors", resolveParams5, null);*/
			/*ResolveParams resolveParams6 = rp;
			resolveParams6.floorDef = TerrainDefOf.Bridge;
			resolveParams6.floorOnlyIfTerrainSupports = new bool?(rp.floorOnlyIfTerrainSupports ?? true);
			resolveParams6.allowBridgeOnAnyImpassableTerrain = new bool?(rp.allowBridgeOnAnyImpassableTerrain ?? true);
			BaseGen.symbolStack.Push("floor", resolveParams6, null);*/
		}

		public static readonly FloatRange DefaultPawnsPoints = new FloatRange(1150f, 1600f);
	}
}