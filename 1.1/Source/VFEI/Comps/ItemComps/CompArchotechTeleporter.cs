using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using RimWorld.Planet;
using UnityEngine;
using Verse.Sound;

namespace VFEI.Comps.ItemComps
{

	[StaticConstructorOnStartup]
	public class CompArchotechTeleporter : CompLaunchable
	{
		private int MaxLaunchDistance = 1000;

		private bool PodsHaveAnyPotentialCaravanOwner
		{
			get
			{
				List<CompTransporter> transportersInGroup = this.TransportersInGroup;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					ThingOwner innerContainer = transportersInGroup[i].innerContainer;
					for (int j = 0; j < innerContainer.Count; j++)
					{
						Pawn pawn = innerContainer[j] as Pawn;
						if (pawn != null && CaravanUtility.IsOwner(pawn, Faction.OfPlayer))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (this.LoadingInProgressOrReadyToLaunch)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "TeleportButton".Translate();
				command_Action.defaultDesc = "TeleportDesc".Translate();
				command_Action.icon = CompLaunchable.LaunchCommandTex;
				command_Action.alsoClickIfOtherInGroupClicked = false;
				command_Action.action = delegate ()
				{
					this.StartChoosingDestination();
				};
				yield return command_Action;
			}
			yield break;
		}

		public override string CompInspectStringExtra()
		{
			if (!this.LoadingInProgressOrReadyToLaunch)
			{
				return null;
			}
			return "ReadyForLaunch".Translate();
		}

		public void StartChoosingDestination()
		{
			CameraJumper.TryJump(CameraJumper.GetWorldTarget(this.parent));
			Find.WorldSelector.ClearSelection();
			int tile = this.parent.Map.Tile;
			bool anyPFC = PodsHaveAnyPotentialCaravanOwner;
			Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true, CompLaunchable.TargeterMouseAttachment, true, delegate
			{
				GenDraw.DrawWorldRadiusRing(tile, this.MaxLaunchDistance);
			}, delegate (GlobalTargetInfo target)
			{
				IEnumerable<FloatMenuOption> transportPodsFloatMenuOptionsAt = this.GetTransportPodsFloatMenuOptionsAt(target.Tile, anyPFC);
				if (!transportPodsFloatMenuOptionsAt.Any<FloatMenuOption>())
				{
					return "";
				}
				if (transportPodsFloatMenuOptionsAt.Count<FloatMenuOption>() == 1)
				{
					if (transportPodsFloatMenuOptionsAt.First<FloatMenuOption>().Disabled)
					{
						GUI.color = ColoredText.RedReadable;
					}
					return transportPodsFloatMenuOptionsAt.First<FloatMenuOption>().Label;
				}
				MapParent mapParent = target.WorldObject as MapParent;
				if (mapParent != null)
				{
					return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap);
				}
				return "ClickToSeeAvailableOrders_Empty".Translate();
			});
		}

		private bool ChoseWorldTarget(GlobalTargetInfo target)
		{
			if (!this.LoadingInProgressOrReadyToLaunch)
			{
				return true;
			}
			if (!target.IsValid && target.WorldObject.Faction != Faction.OfPlayer)
			{
				Messages.Message("TeleportDestInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
				return false;
			}
			bool anyPFC = PodsHaveAnyPotentialCaravanOwner;
			IEnumerable<FloatMenuOption> transportPodsFloatMenuOptionsAt = this.GetTransportPodsFloatMenuOptionsAt(target.Tile, PodsHaveAnyPotentialCaravanOwner);
			if (!transportPodsFloatMenuOptionsAt.Any<FloatMenuOption>())
			{
				if (Find.World.Impassable(target.Tile))
				{
					Messages.Message("TeleportDestInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
					return false;
				}
				this.TryLaunch(target.Tile, null);
				return true;
			}
			else
			{
				if (transportPodsFloatMenuOptionsAt.Count<FloatMenuOption>() == 1)
				{
					if (!transportPodsFloatMenuOptionsAt.First<FloatMenuOption>().Disabled)
					{
						transportPodsFloatMenuOptionsAt.First<FloatMenuOption>().action();
					}
					return false;
				}
				Find.WindowStack.Add(new FloatMenu(transportPodsFloatMenuOptionsAt.ToList<FloatMenuOption>()));
				return false;
			}
		}

		public new void TryLaunch(int destinationTile, TransportPodsArrivalAction arrivalAction)
		{
			if (!this.parent.Spawned)
			{
				Log.Error("Tried to teleport " + this.parent + ", but it's unspawned.", false);
				return;
			}
			List<CompTransporter> transportersInGroup = this.TransportersInGroup;
			if (transportersInGroup == null)
			{
				Log.Error("Tried to teleport " + this.parent + ", but it's not in any group.", false);
				return;
			}
			if (!this.LoadingInProgressOrReadyToLaunch)
			{
				return;
			}
			Map map = this.parent.Map;
			this.Transporter.TryRemoveLord(map);
			int groupID = this.Transporter.groupID;
			for (int i = 0; i < transportersInGroup.Count; i++)
			{
				MoteMaker.MakeStaticMote(this.parent.Position, map, ThingDefOf.Mote_PsycastAreaEffect, 5);
				SoundDefOf.PsychicSootheGlobal.PlayOneShotOnCamera(map);
				CompTransporter compTransporter = transportersInGroup[i];
				ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
				ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
				activeDropPod.Contents = new ActiveDropPodInfo();
				activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
				WorldTeleporter worldTeleporter = (WorldTeleporter)SkyfallerMaker.MakeSkyfaller(ThingDefsVFEI.VFEI_TeleporterTraveler, activeDropPod);
				worldTeleporter.groupID = groupID;
				worldTeleporter.destinationTile = destinationTile;
				worldTeleporter.arrivalAction = arrivalAction;
				compTransporter.CleanUpLoadingVars(map);
				compTransporter.parent.Destroy(DestroyMode.Vanish);
				GenSpawn.Spawn(worldTeleporter, compTransporter.parent.Position, map, WipeMode.Vanish);
			}

			CameraJumper.TryHideWorld();
		}

		public IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptionsAt(int tile, bool anyPawnsCarvaning)
		{
			bool anything = false;
			if (VFEI_TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(this.TransportersInGroup.Cast<IThingHolder>(), tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
			{
				anything = true;
				yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate ()
				{
					this.TryLaunch(tile, new VFEI_TransportPodsArrivalAction_FormCaravan());
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
			}
			List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
			int num;
			for (int i = 0; i < worldObjects.Count; i = num + 1)
			{
				if (worldObjects[i].Tile == tile && worldObjects[i].Faction == Faction.OfPlayer)
				{
					yield return new FloatMenuOption("TeleportHere".Translate(), delegate ()
					{
						this.TryLaunch(tile, null);
					}, MenuOptionPriority.Default, null, null, 0f, null, null);
				}
				else if (worldObjects[i].Tile == tile && worldObjects[i].Faction != Faction.OfPlayer && anyPawnsCarvaning)
				{
					yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate ()
					{
						this.TryLaunch(tile, null);
					}, MenuOptionPriority.Default, null, null, 0f, null, null);
				}
				else if (worldObjects[i].Tile == tile && worldObjects[i].Faction != Faction.OfPlayer && !anyPawnsCarvaning)
				{
					yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate ()
					{
						this.TryLaunch(tile, null);
					}, MenuOptionPriority.Default, null, null, 0f, null, null);
				}
				num = i;
			}
			if (!anything && !Find.World.Impassable(tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
			{
				yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate ()
				{
					this.TryLaunch(tile, null);
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
			}
			yield break;
		}
	}
}
