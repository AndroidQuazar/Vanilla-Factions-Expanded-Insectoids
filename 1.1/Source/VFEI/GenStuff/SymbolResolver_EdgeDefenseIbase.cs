using System;
using Verse;
using Verse.AI.Group;
using RimWorld.BaseGen;
using RimWorld;
using RimWorld.Planet;

namespace VFEI.GenStuff
{
    class SymbolResolver_EdgeDefenseIbase : SymbolResolver
    {
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			Faction faction = rp.faction;
			int width = rp.edgeDefenseWidth.Value;
			CellRect rect = rp.rect;
			for (int j = 0; j < width; j++)
			{
				if (j % 2 == 0)
				{
					ResolveParams rp3 = rp;
					rp3.faction = faction;
					rp3.rect = rect;
					BaseGen.symbolStack.Push("insectoidBaseEdgeRocks", rp3, null);
				}
				rect = rect.ContractedBy(1);
			}
			/*CellRect rect3 = rp.rect.ContractedBy(1);
			for (int l = 0; l < 10; l++)
			{
				ResolveParams rp5 = rp;
				rp5.faction = faction;
				rp5.singleThingDef = ThingDefOf.Hive;
				rp5.rect = rect3;
				rp5.edgeThingAvoidOtherEdgeThings = new bool?(rp.edgeThingAvoidOtherEdgeThings ?? true);
				BaseGen.symbolStack.Push("edgeThing", rp5, null);
			}*/
		}
	}
}
