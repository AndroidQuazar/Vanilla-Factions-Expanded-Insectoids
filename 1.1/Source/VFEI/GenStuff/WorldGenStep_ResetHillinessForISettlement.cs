using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace VFEI
{
    class WorldGenStep_ResetHillinessForISettlement : WorldGenStep
    {
		public override int SeedPart
		{
			get
			{
				return 507014749;
			}
		}

		public override void GenerateFresh(string seed)
		{
			this.ReGenerateFSHilliness();
		}

		private void ReGenerateFSHilliness()
		{
			foreach (WorldObject item in Find.World.worldObjects.AllWorldObjects)
			{
				if (item.Faction.def.defName == "VFEI_Insect" && Find.WorldGrid[item.Tile].hilliness != Hilliness.Mountainous)
				{
					Find.WorldGrid[item.Tile].hilliness = Hilliness.Mountainous;
				}
			}
		}
	}
}
