using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using RimWorld;
using UnityEngine;

namespace VFEI
{
    class WorldTeleporter : Skyfaller, IActiveDropPod, IThingHolder
	{
		public ActiveDropPodInfo Contents
		{
			get
			{
				return ((ActiveDropPod)this.innerContainer[0]).Contents;
			}
			set
			{
				((ActiveDropPod)this.innerContainer[0]).Contents = value;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.groupID, "groupID", 0, false);
			Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
			Scribe_Deep.Look<TransportPodsArrivalAction>(ref this.arrivalAction, "arrivalAction", Array.Empty<object>());
			Scribe_Values.Look<bool>(ref this.alreadyLeft, "alreadyLeft", false, false);
		}

		protected override void LeaveMap()
		{
			if (this.alreadyLeft)
			{
				base.LeaveMap();
				return;
			}
			if (this.groupID < 0)
			{
				Log.Error("Teleporter group ID is " + this.groupID, false);
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			if (this.destinationTile < 0)
			{
				Log.Error("Teleporter destination tile is " + this.destinationTile, false);
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			Lord lord = TransporterUtility.FindLord(this.groupID, base.Map);
			if (lord != null)
			{
				base.Map.lordManager.RemoveLord(lord);
			}
			TravelingTeleporter travelingTeleporter = (TravelingTeleporter)WorldObjectMaker.MakeWorldObject(ThingDefsVFEI.VFEI_TTraveling);
			travelingTeleporter.Tile = base.Map.Tile;
			travelingTeleporter.SetFaction(Faction.OfPlayer);
			travelingTeleporter.destinationTile = this.destinationTile;
			travelingTeleporter.arrivalAction = this.arrivalAction;
			Find.WorldObjects.Add(travelingTeleporter);
			tmpActiveDropPods.Clear();
			tmpActiveDropPods.AddRange(base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveDropPod));
			for (int i = 0; i < tmpActiveDropPods.Count; i++)
			{
				WorldTeleporter worldTeleporter = WorldTeleporter.tmpActiveDropPods[i] as WorldTeleporter;
				if (worldTeleporter != null && worldTeleporter.groupID == this.groupID)
				{
					worldTeleporter.alreadyLeft = true;
					travelingTeleporter.AddPod(worldTeleporter.Contents, true);
					worldTeleporter.Contents = null;
					worldTeleporter.Destroy(DestroyMode.Vanish);
				}
			}
		}

		public int groupID = -1;
		public int destinationTile = -1;
		public TransportPodsArrivalAction arrivalAction;
		private bool alreadyLeft;
		private static List<Thing> tmpActiveDropPods = new List<Thing>();
	}

	public class TravelingTeleporter : WorldObject, IThingHolder
	{
		private Vector3 Start
		{
			get
			{
				return Find.WorldGrid.GetTileCenter(this.initialTile);
			}
		}

		private Vector3 End
		{
			get
			{
				return Find.WorldGrid.GetTileCenter(this.destinationTile);
			}
		}

		public override Vector3 DrawPos
		{
			get
			{
				return Vector3.Slerp(this.Start, this.End, this.traveledPct);
			}
		}

		private float TraveledPctStepPerTick
		{
			get
			{
				Vector3 start = this.Start;
				Vector3 end = this.End;
				if (start == end)
				{
					return 1f;
				}
				float num = GenMath.SphericalDistance(start.normalized, end.normalized);
				if (num == 0f)
				{
					return 1f;
				}
				return TravelSpeed / num;
			}
		}

		private bool PodsHaveAnyPotentialCaravanOwner
		{
			get
			{
				for (int i = 0; i < this.pods.Count; i++)
				{
					ThingOwner innerContainer = this.pods[i].innerContainer;
					for (int j = 0; j < innerContainer.Count; j++)
					{
						Pawn pawn = innerContainer[j] as Pawn;
						if (pawn != null && CaravanUtility.IsOwner(pawn, base.Faction))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool PodsHaveAnyFreeColonist
		{
			get
			{
				for (int i = 0; i < this.pods.Count; i++)
				{
					ThingOwner innerContainer = this.pods[i].innerContainer;
					for (int j = 0; j < innerContainer.Count; j++)
					{
						Pawn pawn = innerContainer[j] as Pawn;
						if (pawn != null && pawn.IsColonist && pawn.HostFaction == null)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public IEnumerable<Pawn> Pawns
		{
			get
			{
				int num;
				for (int i = 0; i < this.pods.Count; i = num + 1)
				{
					ThingOwner things = this.pods[i].innerContainer;
					for (int j = 0; j < things.Count; j = num + 1)
					{
						Pawn pawn = things[j] as Pawn;
						if (pawn != null)
						{
							yield return pawn;
						}
						num = j;
					}
					things = null;
					num = i;
				}
				yield break;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look<ActiveDropPodInfo>(ref this.pods, "pods", LookMode.Deep, Array.Empty<object>());
			Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
			Scribe_Deep.Look<TransportPodsArrivalAction>(ref this.arrivalAction, "arrivalAction", Array.Empty<object>());
			Scribe_Values.Look<bool>(ref this.arrived, "arrived", false, false);
			Scribe_Values.Look<int>(ref this.initialTile, "initialTile", 0, false);
			Scribe_Values.Look<float>(ref this.traveledPct, "traveledPct", 0f, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				for (int i = 0; i < this.pods.Count; i++)
				{
					this.pods[i].parent = this;
				}
			}
		}

		public override void PostAdd()
		{
			base.PostAdd();
			this.initialTile = base.Tile;
		}

		public override void Tick()
		{
			base.Tick();
			this.traveledPct += 1f;
			if (this.traveledPct >= 1f)
			{
				this.traveledPct = 1f;
				this.Arrived();
			}
		}

		private void Arrived()
		{
			if (this.arrived)
			{
				return;
			}
			this.arrived = true;
			if (this.arrivalAction == null || !this.arrivalAction.StillValid(this.pods, this.destinationTile))
			{
				this.arrivalAction = null;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].Tile == this.destinationTile)
					{
						this.arrivalAction = new VFEI_TransportPodsArrivalAction_LandInSpecificCell(maps[i].Parent, DropCellFinder.RandomDropSpot(maps[i]));
						break;
					}
				}
				if (this.arrivalAction == null)
				{
					if (VFEI_TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(this.pods, this.destinationTile))
					{
						this.arrivalAction = new VFEI_TransportPodsArrivalAction_FormCaravan();
					}
					else
					{
						List<Caravan> caravans = Find.WorldObjects.Caravans;
						for (int j = 0; j < caravans.Count; j++)
						{
							if (caravans[j].Tile == this.destinationTile && VFEI_TransportPodsArrivalAction_GiveToCaravan.CanGiveTo(this.pods, caravans[j]))
							{
								this.arrivalAction = new VFEI_TransportPodsArrivalAction_GiveToCaravan(caravans[j]);
								break;
							}
						}
					}
				}
			}
			if (this.arrivalAction != null && this.arrivalAction.ShouldUseLongEvent(this.pods, this.destinationTile))
			{
				LongEventHandler.QueueLongEvent(delegate ()
				{
					this.DoArrivalAction();
				}, "GeneratingMapForNewEncounter", false, null, true);
				return;
			}
			this.DoArrivalAction();
		}

		private void DoArrivalAction()
		{
			for (int i = 0; i < this.pods.Count; i++)
			{
				this.pods[i].savePawnsWithReferenceMode = false;
				this.pods[i].parent = null;
			}
			if (this.arrivalAction != null)
			{
				try
				{
					this.arrivalAction.Arrived(this.pods, this.destinationTile);
				}
				catch (Exception arg)
				{
					Log.Error("Exception in transport pods arrival action: " + arg, false);
				}
				this.arrivalAction = null;
			}
			else
			{
				for (int j = 0; j < this.pods.Count; j++)
				{
					for (int k = 0; k < this.pods[j].innerContainer.Count; k++)
					{
						Pawn pawn = this.pods[j].innerContainer[k] as Pawn;
						if (pawn != null && (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer))
						{
							PawnBanishUtility.Banish(pawn, this.destinationTile);
						}
					}
				}
				for (int l = 0; l < this.pods.Count; l++)
				{
					this.pods[l].innerContainer.ClearAndDestroyContentsOrPassToWorld(DestroyMode.Vanish);
				}
				Messages.Message("MessageTransportPodsArrivedAndLost".Translate(), new GlobalTargetInfo(this.destinationTile), MessageTypeDefOf.NegativeEvent, true);
			}
			this.pods.Clear();
			this.Destroy();
		}

		public void AddPod(ActiveDropPodInfo contents, bool justLeftTheMap)
		{
			contents.parent = this;
			this.pods.Add(contents);
			ThingOwner innerContainer = contents.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				Pawn pawn = innerContainer[i] as Pawn;
				if (pawn != null && !pawn.IsWorldPawn())
				{
					if (!base.Spawned)
					{
						Log.Warning("Passing pawn " + pawn + " to world, but the TravelingTransportPod is not spawned. This means that WorldPawns can discard this pawn which can cause bugs.", false);
					}
					if (justLeftTheMap)
					{
						pawn.ExitMap(false, Rot4.Invalid);
					}
					else
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
					}
				}
			}
			contents.savePawnsWithReferenceMode = true;
		}

		public bool ContainsPawn(Pawn p)
		{
			for (int i = 0; i < this.pods.Count; i++)
			{
				if (this.pods[i].innerContainer.Contains(p))
				{
					return true;
				}
			}
			return false;
		}

		/* private void Arrived()
		{
			if (this.arrived)
			{
				return;
			}
			this.arrived = true;
			if (this.arrivalAction == null || !this.arrivalAction.StillValid(this.pods, this.destinationTile))
			{
				this.arrivalAction = null;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].Tile == this.destinationTile)
					{
						this.arrivalAction = new VFEI_TransportPodsArrivalAction_LandInSpecificCell(maps[i].Parent, DropCellFinder.RandomDropSpot(maps[i]));
						break;
					}
				}
				if (this.arrivalAction == null)
				{
					if (VFEI_TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(this.pods, this.destinationTile))
					{
						this.arrivalAction = new VFEI_TransportPodsArrivalAction_FormCaravan();
					}
					else
					{
						List<Caravan> caravans = Find.WorldObjects.Caravans;
						for (int j = 0; j < caravans.Count; j++)
						{
							if (caravans[j].Tile == this.destinationTile && VFEI_TransportPodsArrivalAction_GiveToCaravan.CanGiveTo(this.pods, caravans[j]))
							{
								this.arrivalAction = new VFEI_TransportPodsArrivalAction_GiveToCaravan(caravans[j]);
								break;
							}
						}
					}
				}
			}
			if (this.arrivalAction != null && this.arrivalAction.ShouldUseLongEvent(this.pods, this.destinationTile))
			{
				LongEventHandler.QueueLongEvent(delegate ()
				{
					this.DoArrivalAction();
				}, "GeneratingMapForNewEncounter", false, null, true);
				return;
			}
			this.DoArrivalAction();
		}

		private void DoArrivalAction()
		{
			for (int i = 0; i < this.pods.Count; i++)
			{
				this.pods[i].savePawnsWithReferenceMode = false;
				this.pods[i].parent = null;
			}
			if (this.arrivalAction != null)
			{
				try
				{
					this.arrivalAction.Arrived(this.pods, this.destinationTile);
				}
				catch (Exception arg)
				{
					Log.Error("Exception in teleporter action: " + arg, false);
				}
				this.arrivalAction = null;
			}
			else
			{
				for (int j = 0; j < this.pods.Count; j++)
				{
					for (int k = 0; k < this.pods[j].innerContainer.Count; k++)
					{
						Pawn pawn = this.pods[j].innerContainer[k] as Pawn;
						if (pawn != null && (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer))
						{
							PawnBanishUtility.Banish(pawn, this.destinationTile);
						}
					}
				}
				for (int l = 0; l < this.pods.Count; l++)
				{
					this.pods[l].innerContainer.ClearAndDestroyContentsOrPassToWorld(DestroyMode.Vanish);
				}
				Messages.Message("TeleportDestroyed".Translate(), new GlobalTargetInfo(this.destinationTile), MessageTypeDefOf.NegativeEvent, true);
			}
			this.pods.Clear();
			this.Destroy();
		}*/

		public ThingOwner GetDirectlyHeldThings()
		{
			return null;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
			for (int i = 0; i < this.pods.Count; i++)
			{
				outChildren.Add(this.pods[i]);
			}
		}

		public int destinationTile = -1;
		public TransportPodsArrivalAction arrivalAction;
		private List<ActiveDropPodInfo> pods = new List<ActiveDropPodInfo>();
		private bool arrived;
		private int initialTile = -1;
		private float traveledPct;
		private const float TravelSpeed = 1f;
	}

	public class VFEI_TransportPodsArrivalAction_LandInSpecificCell : TransportPodsArrivalAction
	{
		public VFEI_TransportPodsArrivalAction_LandInSpecificCell()
		{
		}

		public VFEI_TransportPodsArrivalAction_LandInSpecificCell(MapParent mapParent, IntVec3 cell)
		{
			this.mapParent = mapParent;
			this.cell = cell;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<MapParent>(ref this.mapParent, "mapParent", false);
			Scribe_Values.Look<IntVec3>(ref this.cell, "cell", default(IntVec3), false);
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (this.mapParent != null && this.mapParent.Tile != destinationTile)
			{
				return false;
			}
			return VFEI_TransportPodsArrivalAction_LandInSpecificCell.CanLandInSpecificCell(pods, this.mapParent);
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			Thing lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
			foreach (ActiveDropPodInfo podInfo in pods)
			{
				for (int i = podInfo.innerContainer.Count - 1; i >= 0; i--)
				{
					GenPlace.TryPlaceThing(podInfo.innerContainer[i], this.cell, this.mapParent.Map, ThingPlaceMode.Near, delegate (Thing thing, int count)
					{
						PawnUtility.RecoverFromUnwalkablePositionOrKill(thing.Position, thing.Map);
					}, null, podInfo.innerContainer[i].def.defaultPlacingRot);
				}
			}
			// TransportPodsArrivalActionUtility.DropTravelingTransportPods(pods, this.cell, this.mapParent.Map);
			Messages.Message("TeleportDone".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion, true);
		}

		public static bool CanLandInSpecificCell(IEnumerable<IThingHolder> pods, MapParent mapParent)
		{
			return mapParent != null && mapParent.Spawned && mapParent.HasMap && (!mapParent.EnterCooldownBlocksEntering() || FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(mapParent.EnterCooldownDaysLeft().ToString("0.#"))));
		}

		private MapParent mapParent;
		private IntVec3 cell;
	}

	public class VFEI_TransportPodsArrivalAction_FormCaravan : TransportPodsArrivalAction
	{
		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			return CanFormCaravanAt(pods, destinationTile);
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			tmpPawns.Clear();
			for (int i = 0; i < pods.Count; i++)
			{
				ThingOwner innerContainer = pods[i].innerContainer;
				for (int j = innerContainer.Count - 1; j >= 0; j--)
				{
					Pawn pawn = innerContainer[j] as Pawn;
					if (pawn != null)
					{
						tmpPawns.Add(pawn);
						innerContainer.Remove(pawn);
					}
				}
			}
			int startingTile;
			if (!GenWorldClosest.TryFindClosestPassableTile(tile, out startingTile))
			{
				startingTile = tile;
			}
			Caravan caravan = CaravanMaker.MakeCaravan(tmpPawns, Faction.OfPlayer, startingTile, true);
			for (int k = 0; k < pods.Count; k++)
			{
				tmpContainedThings.Clear();
				tmpContainedThings.AddRange(pods[k].innerContainer);
				for (int l = 0; l < tmpContainedThings.Count; l++)
				{
					pods[k].innerContainer.Remove(tmpContainedThings[l]);
					CaravanInventoryUtility.GiveThing(caravan, tmpContainedThings[l]);
				}
			}
			tmpPawns.Clear();
			tmpContainedThings.Clear();
			Messages.Message("TeleportDone".Translate(), caravan, MessageTypeDefOf.TaskCompletion, true);
		}

		public static bool CanFormCaravanAt(IEnumerable<IThingHolder> pods, int tile)
		{
			return TransportPodsArrivalActionUtility.AnyPotentialCaravanOwner(pods, Faction.OfPlayer) && !Find.World.Impassable(tile);
		}

		private static List<Pawn> tmpPawns = new List<Pawn>();
		private static List<Thing> tmpContainedThings = new List<Thing>();
	}

	public class VFEI_TransportPodsArrivalAction_GiveToCaravan : TransportPodsArrivalAction
	{
		public VFEI_TransportPodsArrivalAction_GiveToCaravan()
		{
		}

		public VFEI_TransportPodsArrivalAction_GiveToCaravan(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Caravan>(ref this.caravan, "caravan", false);
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (this.caravan != null && !Find.WorldGrid.IsNeighborOrSame(this.caravan.Tile, destinationTile))
			{
				return false;
			}
			return VFEI_TransportPodsArrivalAction_GiveToCaravan.CanGiveTo(pods, this.caravan);
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			for (int i = 0; i < pods.Count; i++)
			{
				VFEI_TransportPodsArrivalAction_GiveToCaravan.tmpContainedThings.Clear();
				VFEI_TransportPodsArrivalAction_GiveToCaravan.tmpContainedThings.AddRange(pods[i].innerContainer);
				for (int j = 0; j < VFEI_TransportPodsArrivalAction_GiveToCaravan.tmpContainedThings.Count; j++)
				{
					pods[i].innerContainer.Remove(VFEI_TransportPodsArrivalAction_GiveToCaravan.tmpContainedThings[j]);
					this.caravan.AddPawnOrItem(VFEI_TransportPodsArrivalAction_GiveToCaravan.tmpContainedThings[j], true);
				}
			}
			VFEI_TransportPodsArrivalAction_GiveToCaravan.tmpContainedThings.Clear();
			Messages.Message("TeleportDoneCAdded".Translate(this.caravan.Name), this.caravan, MessageTypeDefOf.TaskCompletion, true);
		}

		public static FloatMenuAcceptanceReport CanGiveTo(IEnumerable<IThingHolder> pods, Caravan caravan)
		{
			return caravan != null && caravan.Spawned && caravan.IsPlayerControlled;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchable representative, IEnumerable<IThingHolder> pods, Caravan caravan)
		{
			return TransportPodsArrivalActionUtility.GetFloatMenuOptions<VFEI_TransportPodsArrivalAction_GiveToCaravan>(() => VFEI_TransportPodsArrivalAction_GiveToCaravan.CanGiveTo(pods, caravan), () => new VFEI_TransportPodsArrivalAction_GiveToCaravan(caravan), "GiveToCaravan".Translate(caravan.Label), representative, caravan.Tile, null);
		}

		private Caravan caravan;
		private static List<Thing> tmpContainedThings = new List<Thing>();
	}
}
