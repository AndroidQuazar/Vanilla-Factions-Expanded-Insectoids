using System;
using Verse;
using RimWorld;
using RimWorld.BaseGen;

namespace VFEI.Other
{
    class ScenPart_Settlement : ScenPart
    {
        public override void PostMapGenerate(Map map)
        {
			IntRange SettlementSizeRange = new IntRange(34, 38);
			int randomInRange = SettlementSizeRange.RandomInRange;
			int randomInRange2 = SettlementSizeRange.RandomInRange;
			IntVec3 c = map.Center;
			CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
			rect.ClipInsideMap(map);
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = rect;
			resolveParams.faction = Faction.OfPlayer;
			BaseGen.globalSettings.map = map;
			BaseGen.globalSettings.minBuildings = 1;
			BaseGen.globalSettings.minBarracks = 1;
			BaseGen.symbolStack.Push("settlementNoPawns", resolveParams, null);
			BaseGen.Generate();
		}
    }
}
