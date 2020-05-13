using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Verse;
using Verse.Noise;
using RimWorld;

namespace VFEI.GenStuff
{
    class GenStep_IbaseCaves : GenStep
    {
		public override int SeedPart
		{
			get
			{
				return 73858749;
			}
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			/*if (!Find.World.HasCaves(map.Tile) || map.ParentFaction == null || map.ParentFaction.def.defName != "VFEI_Insect")
			{
				Log.Message("Quit");
				return;
			}*/
			Log.Message("test");
			this.directionNoise = new Perlin(0.0020500000100582838, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
			this.tunnelWidthNoise = new Perlin(0.019999999552965164, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
			this.tunnelWidthNoise = new ScaleBias(0.4, 1.0, this.tunnelWidthNoise);
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			BoolGrid visited = new BoolGrid(map);
			List<IntVec3> rockCells = new List<IntVec3>();
			foreach (IntVec3 cell in map.AllCells)
			{
				if (!visited[cell] && this.IsRock(cell, elevation, map))
				{
					rockCells.Clear();
					map.floodFiller.FloodFill(cell, (IntVec3 x) => IsRock(x, elevation, map), delegate (IntVec3 x)
					{
						visited[x] = true;
						rockCells.Add(x);
					});
					Trim(rockCells, map);
					RemoveSmallDisconnectedSubGroups(rockCells, map);
					if (rockCells.Count >= 500)
					{
						StartWithTunnel(rockCells, map);
					}
				}
			}
			this.SmoothGenerated(map);
		}

		void Trim(List<IntVec3> group, Map map)
		{
			GenMorphology.Open(group, 6, map);
		}

		bool IsRock(IntVec3 c, MapGenFloatGrid elevation, Map map)
		{
			return c.InBounds(map) && elevation[c] > 0.7f;
		}

		void StartWithTunnel(List<IntVec3> group, Map map)
		{
			int num = GenMath.RoundRandom((float)group.Count * 2f / 10000f);
			num = Mathf.Min(num, 6);
			float num2 = GenStep_IbaseCaves.TunnelsWidthPerRockCount.Evaluate((float)group.Count);
			for (int i = 0; i < num; i++)
			{
				IntVec3 start = IntVec3.Invalid;
				float num3 = -1f;
				float dir = -1f;
				float num4 = -1f;
				for (int j = 0; j < 10; j++)
				{
					IntVec3 intVec = this.FindRandomEdgeCellForTunnel(group, map);
					float distToCave = this.GetDistToCave(intVec, group, map, 40f, false);
					float num6;
					float num5 = this.FindBestInitialDir(intVec, group, out num6);
					bool flag = !start.IsValid || distToCave > num3 || (distToCave == num3 && num6 > num4);
					if (flag)
					{
						start = intVec;
						num3 = distToCave;
						dir = num5;
						num4 = num6;
					}
				}
				float width = num2 * Rand.Range(0.6f, 0.9f);
				this.Dig(start, dir, width, group, map, GenStep_IbaseCaves.BranchType.Normal, 1, null);
			}
		}

		IntVec3 FindRandomEdgeCellForTunnel(List<IntVec3> group, Map map)
		{
			MapGenFloatGrid caves = MapGenerator.Caves;
			IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
			GenStep_IbaseCaves.tmpCells.Clear();
			GenStep_IbaseCaves.tmpGroupSet.Clear();
			GenStep_IbaseCaves.tmpGroupSet.AddRange(group);
			for (int i = 0; i < group.Count; i++)
			{
				bool flag = group[i].DistanceToEdge(map) >= 3 && caves[group[i]] <= 0f;
				if (flag)
				{
					for (int j = 0; j < 4; j++)
					{
						IntVec3 item = group[i] + cardinalDirections[j];
						bool flag2 = !GenStep_IbaseCaves.tmpGroupSet.Contains(item);
						if (flag2)
						{
							GenStep_IbaseCaves.tmpCells.Add(group[i]);
							break;
						}
					}
				}
			}
			bool flag3 = !GenStep_IbaseCaves.tmpCells.Any<IntVec3>();
			IntVec3 result;
			if (flag3)
			{
				Log.Warning("Could not find any valid edge cell.", false);
				result = group.RandomElement<IntVec3>();
			}
			else
			{
				result = GenStep_IbaseCaves.tmpCells.RandomElement<IntVec3>();
			}
			return result;
		}

		float FindBestInitialDir(IntVec3 start, List<IntVec3> group, out float dist)
		{
			float num = (float)this.GetDistToNonRock(start, group, IntVec3.East, 40);
			float num2 = (float)this.GetDistToNonRock(start, group, IntVec3.West, 40);
			float num3 = (float)this.GetDistToNonRock(start, group, IntVec3.South, 40);
			float num4 = (float)this.GetDistToNonRock(start, group, IntVec3.North, 40);
			float num5 = (float)this.GetDistToNonRock(start, group, IntVec3.NorthWest, 40);
			float num6 = (float)this.GetDistToNonRock(start, group, IntVec3.NorthEast, 40);
			float num7 = (float)this.GetDistToNonRock(start, group, IntVec3.SouthWest, 40);
			float num8 = (float)this.GetDistToNonRock(start, group, IntVec3.SouthEast, 40);
			dist = Mathf.Max(new float[]
			{
				num,
				num2,
				num3,
				num4,
				num5,
				num6,
				num7,
				num8
			});
			return GenMath.MaxByRandomIfEqual<float>(0f, num + num8 / 2f + num6 / 2f, 45f, num8 + num3 / 2f + num / 2f, 90f, num3 + num8 / 2f + num7 / 2f, 135f, num7 + num3 / 2f + num2 / 2f, 180f, num2 + num7 / 2f + num5 / 2f, 225f, num5 + num4 / 2f + num2 / 2f, 270f, num4 + num6 / 2f + num5 / 2f, 315f, num6 + num4 / 2f + num / 2f, 0.0001f);
		}

		void Dig(IntVec3 start, float dir, float width, List<IntVec3> group, Map map, GenStep_IbaseCaves.BranchType branchType, int depth, HashSet<IntVec3> visited = null)
		{
			Vector3 vector = start.ToVector3Shifted();
			IntVec3 intVec = start;
			float num = 0f;
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			MapGenFloatGrid caves = MapGenerator.Caves;
			int num2 = 0;
			int num3 = 0;
			float num4 = Rand.Range(0.019f, 0.044f);
			bool flag = visited == null;
			if (flag)
			{
				visited = new HashSet<IntVec3>();
			}
			GenStep_IbaseCaves.tmpGroupSet.Clear();
			GenStep_IbaseCaves.tmpGroupSet.AddRange(group);
			int num5 = 0;
			for (; ; )
			{
				bool flag2 = branchType == GenStep_IbaseCaves.BranchType.Normal;
				if (flag2)
				{
					bool flag3 = num5 - num2 >= 30 && Rand.Chance(0.020f);
					if (flag3)
					{
						GenStep_IbaseCaves.BranchType branchType2 = this.RandomBranchTypeByChance();
						float num6 = this.CalculateBranchWidth(branchType, width);
						bool flag4 = num6 > 3.4f;
						if (flag4)
						{
							this.DigInBestDirection(intVec, dir, new FloatRange(-90f, -40f), num6, group, map, branchType2, depth, visited);
							num2 = num5;
						}
					}
					bool flag5 = num5 - num3 >= 30 && Rand.Chance(0.020f);
					if (flag5)
					{
						GenStep_IbaseCaves.BranchType branchType3 = this.RandomBranchTypeByChance();
						float num7 = this.CalculateBranchWidth(branchType, width);
						bool flag6 = num7 > 3.4f;
						if (flag6)
						{
							this.DigInBestDirection(intVec, dir, new FloatRange(40f, 90f), num7, group, map, branchType3, depth, visited);
							num3 = num5;
						}
					}
				}
				bool flag7;
				this.SetCaveAround(intVec, width, map, visited, out flag7);
				bool flag8 = flag7;
				if (flag8)
				{
					break;
				}
				bool flag9 = branchType == GenStep_IbaseCaves.BranchType.Room && num5 >= 25;
				if (flag9)
				{
					break;
				}
				while (IntVec3Utility.ToIntVec3(vector) == intVec)
				{
					vector += Vector3Utility.FromAngleFlat(dir) * 0.5f;
					num += 0.5f;
				}
				IntVec3 intVec2 = IntVec3Utility.ToIntVec3(vector);
				bool flag10 = !GenStep_IbaseCaves.tmpGroupSet.Contains(intVec2);
				if (flag10)
				{
					break;
				}
				IntVec3 intVec3 = new IntVec3(intVec.x, 0, intVec2.z);
				bool flag11 = this.IsRock(intVec3, elevation, map);
				if (flag11)
				{
					caves[intVec3] = Math.Max(caves[intVec3], width);
					visited.Add(intVec3);
				}
				bool flag12 = branchType == GenStep_IbaseCaves.BranchType.Tunnel;
				if (flag12)
				{
					width -= num4 * 0.2f;
				}
				else
				{
					width -= num4;
				}
				bool flag13 = width < 3.4f;
				if (flag13)
				{
					break;
				}
				intVec = intVec2;
				dir += (float)this.directionNoise.GetValue((double)(num * 60f), (double)((float)start.x * 200f), (double)((float)start.z * 200f)) * 6f;
				num5++;
			}
		}

		void DigInBestDirection(IntVec3 curIntVec, float curDir, FloatRange dirOffset, float width, List<IntVec3> group, Map map, GenStep_IbaseCaves.BranchType branchType, int depth, HashSet<IntVec3> visited = null)
		{
			float dir = -1f;
			int num = -1;
			for (int i = 0; i < 6; i++)
			{
				float num2 = curDir + dirOffset.RandomInRange;
				int distToNonRock = this.GetDistToNonRock(curIntVec, group, num2, 50);
				bool flag = distToNonRock > num;
				if (flag)
				{
					num = distToNonRock;
					dir = num2;
				}
			}
			bool flag2 = num < 15;
			if (!flag2)
			{
				this.Dig(curIntVec, dir, width, group, map, branchType, depth + 1, visited);
			}
		}

		void SetCaveAround(IntVec3 center, float tunnelWidth, Map map, HashSet<IntVec3> visited, out bool hitAnotherTunnel)
		{
			hitAnotherTunnel = false;
			int num = GenRadial.NumCellsInRadius(tunnelWidth / 2f * this.tunnelWidthNoise.GetValue(center));
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			MapGenFloatGrid caves = MapGenerator.Caves;
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = center + GenRadial.RadialPattern[i];
				bool flag = this.IsRock(intVec, elevation, map);
				if (flag)
				{
					bool flag2 = caves[intVec] > 0f && !visited.Contains(intVec);
					if (flag2)
					{
						hitAnotherTunnel = true;
					}
					caves[intVec] = Math.Max(caves[intVec], tunnelWidth);
					visited.Add(intVec);
				}
			}
		}

		private BranchType RandomBranchTypeByChance()
		{
			float rand = Rand.Value;
			if (rand < 0.15f)
			{
				return BranchType.Room;
			}
			if (rand < 0.15f + 0.25f)
			{
				return BranchType.Tunnel;
			}
			return BranchType.Normal;
		}

		private float CalculateBranchWidth(BranchType branchType, float prevWidth)
		{
			if (branchType == BranchType.Room)
			{
				return Rand.Range(20f, 30f);
			}
			if (branchType == BranchType.Tunnel)
			{
				return Rand.Range(20f, 30f);
			}
			return prevWidth * Rand.Range(20f, 30f);
		}

		private int GetDistToNonRock(IntVec3 from, List<IntVec3> group, IntVec3 offset, int maxDist)
		{
			groupSet.Clear();
			groupSet.AddRange(group);
			for (int i = 0; i <= maxDist; i++)
			{
				IntVec3 item = from + offset * i;
				if (!groupSet.Contains(item))
				{
					return i;
				}
			}
			return maxDist;
		}

		private int GetDistToNonRock(IntVec3 from, List<IntVec3> group, float dir, int maxDist)
		{
			groupSet.Clear();
			groupSet.AddRange(group);
			Vector3 a = Vector3Utility.FromAngleFlat(dir);
			for (int i = 0; i <= maxDist; i++)
			{
				IntVec3 item = (from.ToVector3Shifted() + a * i).ToIntVec3();
				if (!groupSet.Contains(item))
				{
					return i;
				}
			}
			return maxDist;
		}

		private float GetDistToCave(IntVec3 cell, List<IntVec3> group, Map map, float maxDist, bool treatOpenSpaceAsCave)
		{
			MapGenFloatGrid caves = MapGenerator.Caves;
			tmpGroupSet.Clear();
			tmpGroupSet.AddRange(group);
			int cellCount = GenRadial.NumCellsInRadius(maxDist);
			IntVec3[] radialPattern = GenRadial.RadialPattern;
			for (int i = 0; i < cellCount; i++)
			{
				IntVec3 loc = cell + radialPattern[i];
				if ((treatOpenSpaceAsCave && !tmpGroupSet.Contains(loc)) || (loc.InBounds(map) && caves[loc] > 0f))
				{
					return cell.DistanceTo(loc);
				}
			}
			return maxDist;
		}

		private void RemoveSmallDisconnectedSubGroups(List<IntVec3> group, Map map)
		{
			groupSet.Clear();
			groupSet.AddRange(group);
			groupVisited.Clear();
			for (int i = 0; i < group.Count; i++)
			{
				if (!groupVisited.Contains(group[i]) && groupSet.Contains(group[i]))
				{
					subGroup.Clear();
					map.floodFiller.FloodFill(group[i], (IntVec3 x) => groupSet.Contains(x), delegate (IntVec3 x)
					{
						subGroup.Add(x);
						groupVisited.Add(x);
					});
					if (subGroup.Count < 500 || (float)subGroup.Count < 0.05f * group.Count)
					{
						for (int j = 0; j < subGroup.Count; j++)
						{
							groupSet.Remove(subGroup[j]);
						}
					}
				}
			}
			group.Clear();
			group.AddRange(groupSet);
		}

		private void SmoothGenerated(Map map)
		{
			MapGenFloatGrid caves = MapGenerator.Caves;
			List<IntVec3> caveCells = new List<IntVec3>();
			foreach (IntVec3 cell in map.AllCells)
			{
				if (caves[cell] > 0f)
				{
					caveCells.Add(cell);
				}
			}
			GenMorphology.Close(caveCells, 3, map);
			foreach (IntVec3 cell in map.AllCells)
			{
				if (cell.CloseToEdge(map, 3)) // Skip changing caves near the edge
				{
					continue;
				}
				if (caveCells.Contains(cell)) // Cave spot on cell (cave should be there)
				{
					if (caves[cell] <= 0f) // No existing cave
					{
						caves[cell] = 1f; // Add new cave spot
					}
				}
				else // No cave spot on cell (cave should not be there)
				{
					if (caves[cell] > 0f) // Old existing cave
					{
						caves[cell] = 0f; // Remove cave spot
					}
				}
			}
		}

		private enum BranchType
		{
			Normal,
			Room,
			Tunnel
		};

		const float DirectionNoiseFrequency = 0.00205f;
		const float TunnelWidthNoiseFrequency = 0.02f;
		private static readonly SimpleCurve TunnelsWidthPerRockCount = new SimpleCurve
		{
			{
				new CurvePoint(300f, 2.5f),
				true
			},
			{
				new CurvePoint(600f, 4.2f),
				true
			},
			{
				new CurvePoint(6000f, 9.5f),
				true
			},
			{
				new CurvePoint(30000f, 15.5f),
				true
			}
		};
		private ModuleBase directionNoise;
		private ModuleBase tunnelWidthNoise;
		private static HashSet<IntVec3> groupSet = new HashSet<IntVec3>();
		private static HashSet<IntVec3> groupVisited = new HashSet<IntVec3>();
		private static List<IntVec3> subGroup = new List<IntVec3>();
		private static List<IntVec3> tmpCells = new List<IntVec3>();
		private static HashSet<IntVec3> tmpGroupSet = new HashSet<IntVec3>();
	}
}
