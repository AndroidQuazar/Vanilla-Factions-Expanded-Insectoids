using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEI
{
    [StaticConstructorOnStartup]
    class CompTeleporter : ThingComp
    {
        private CompTransporter cachedCompTransporter;

        public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true);

        private static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", true);

        private const float FuelPerTile = 2.25f;

        private bool LoadingInProgressOrReadyToLaunch = true;

        /*public bool LoadingInProgressOrReadyToLaunch
        {
            get
            {
                return this.Transporter.LoadingInProgressOrReadyToLaunch;
            }
        }

        public bool AnythingLeftToLoad
        {
            get
            {
                return this.Transporter.AnythingLeftToLoad;
            }
        }

        public Thing FirstThingLeftToLoad
        {
            get
            {
                return this.Transporter.FirstThingLeftToLoad;
            }
        }

        public List<CompTransporter> TransportersInGroup
        {
            get
            {
                return this.Transporter.TransportersInGroup(this.parent.Map);
            }
        }

        public bool AnyInGroupHasAnythingLeftToLoad
        {
            get
            {
                return this.Transporter.AnyInGroupHasAnythingLeftToLoad;
            }
        }

        public Thing FirstThingLeftToLoadInGroup
        {
            get
            {
                return this.Transporter.FirstThingLeftToLoadInGroup;
            }
        }*/

        public CompTransporter Transporter
        {
            get
            {
                if (this.cachedCompTransporter == null)
                {
                    this.cachedCompTransporter = this.parent.GetComp<CompTransporter>();
                }
                return this.cachedCompTransporter;
            }
        }

        private int MaxLaunchDistance
        {
            get
            {
                //if (!this.loadinginprogressorreadytolaunch)
                //{
                //    return 0;
                //}
                return CompLaunchable.MaxLaunchDistanceAtFuelLevel(999999);
            }
        }

        private int MaxLaunchDistanceEverPossible
        {
            get
            {
                //if (!this.LoadingInProgressOrReadyToLaunch)
                //{
                //    return 0;
                //}
                //List<CompTransporter> transportersInGroup = this.TransportersInGroup;
                //float num = 0f;
                //for (int i = 0; i < transportersInGroup.Count; i++)
                //{
                //    Building fuelingPortSource = transportersInGroup[i].Launchable.FuelingPortSource;
                //    if (fuelingPortSource != null)
                //    {
                //        num = Mathf.Max(num, fuelingPortSource.GetComp<CompRefuelable>().Props.fuelCapacity);
                //    }
                //}
                return CompLaunchable.MaxLaunchDistanceAtFuelLevel(99999);
            }
        }

        private bool PodsHaveAnyPotentialCaravanOwner
        {
            get
            {
                //List<CompTransporter> transportersInGroup = this.TransportersInGroup;
                //for (int i = 0; i < transportersInGroup.Count; i++)
                //{
                //    ThingOwner innerContainer = transportersInGroup[i].innerContainer;
                //    for (int j = 0; j < innerContainer.Count; j++)
                //    {
                //        Pawn pawn = innerContainer[j] as Pawn;
                //        if (pawn != null && CaravanUtility.IsOwner(pawn, Faction.OfPlayer))
                //        {
                //            return true;
                //        }
                //    }
                //}
                return false;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
            if (this.LoadingInProgressOrReadyToLaunch)
            {
                Command_Action launch = new Command_Action();
                launch.defaultLabel = "CommandLaunchGroup".Translate();
                launch.defaultDesc = "CommandLaunchGroupDesc".Translate();
                launch.icon = CompTeleporter.LaunchCommandTex;
                launch.alsoClickIfOtherInGroupClicked = false;
                launch.action = delegate ()
                {
                    //if (this.AnyInGroupHasAnythingLeftToLoad)
                    //{
                    //    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate(this.FirstThingLeftToLoadInGroup.LabelCapNoCount, this.FirstThingLeftToLoadInGroup), new Action(this.StartChoosingDestination), false, null));
                    //}
                    //else
                    //{
                    //    this.StartChoosingDestination();
                    //}
                    this.StartChoosingDestination();
                };
                yield return launch;
            }
            yield break;
        }

        public override string CompInspectStringExtra()
        {
            //if (!this.LoadingInProgressOrReadyToLaunch)
            //{
            //    return null;
            //}
            //if (this.AnyInGroupHasAnythingLeftToLoad)
            //{
            //    return "NotReadyForLaunch".Translate() + ": " + "TransportPodInGroupHasSomethingLeftToLoad".Translate() + ".";
            //}
            return "ReadyForLaunch".Translate();
        }

        private void StartChoosingDestination()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(this.parent));
            Find.WorldSelector.ClearSelection();
            int tile = this.parent.Map.Tile;
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true, CompLaunchable.TargeterMouseAttachment, true, delegate
            {
                GenDraw.DrawWorldRadiusRing(tile, this.MaxLaunchDistance);
            }, delegate (GlobalTargetInfo target)
            {
                if (!target.IsValid)
                {
                    return null;
                }
                int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile, true, int.MaxValue);
                if (num > this.MaxLaunchDistance)
                {
                    GUI.color = Color.red;
                    if (num > this.MaxLaunchDistanceEverPossible)
                    {
                        return "TransportPodDestinationBeyondMaximumRange".Translate();
                    }
                    return "TransportPodNotEnoughFuel".Translate();
                }
                else
                {
                    IEnumerable<FloatMenuOption> teleporterFloatMenuOptionsAt = this.GetTeleporterFloatMenuOptionsAt(target.Tile);
                    if (!teleporterFloatMenuOptionsAt.Any<FloatMenuOption>())
                    {
                        return string.Empty;
                    }
                    if (teleporterFloatMenuOptionsAt.Count<FloatMenuOption>() == 1)
                    {
                        if (teleporterFloatMenuOptionsAt.First<FloatMenuOption>().Disabled)
                        {
                            GUI.color = Color.red;
                        }
                        return teleporterFloatMenuOptionsAt.First<FloatMenuOption>().Label;
                    }
                    MapParent mapParent = target.WorldObject as MapParent;
                    if (mapParent != null)
                    {
                        return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap);
                    }
                    return "ClickToSeeAvailableOrders_Empty".Translate();
                }
            });
        }

        private bool ChoseWorldTarget(GlobalTargetInfo target)
        {
            //if (!this.LoadingInProgressOrReadyToLaunch)
            //{
            //    return true;
            //}
            if (!target.IsValid)
            {
                Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            int num = Find.WorldGrid.TraversalDistanceBetween(this.parent.Map.Tile, target.Tile, true, int.MaxValue);
            if (num > this.MaxLaunchDistance)
            {
                Messages.Message("MessageTransportPodsDestinationIsTooFar".Translate(CompLaunchable.FuelNeededToLaunchAtDist((float)num).ToString("0.#")), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            IEnumerable<FloatMenuOption> teloporterFloatMenuOptionsAt = this.GetTeleporterFloatMenuOptionsAt(target.Tile);
            if (!teloporterFloatMenuOptionsAt.Any<FloatMenuOption>())
            {
                if (Find.World.Impassable(target.Tile))
                {
                    Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                    return false;
                }
                this.TryLaunch(target.Tile, null);
                return true;
            }
            else
            {
                if (teloporterFloatMenuOptionsAt.Count<FloatMenuOption>() == 1)
                {
                    if (!teloporterFloatMenuOptionsAt.First<FloatMenuOption>().Disabled)
                    {
                        teloporterFloatMenuOptionsAt.First<FloatMenuOption>().action();
                    }
                    return false;
                }
                Find.WindowStack.Add(new FloatMenu(teloporterFloatMenuOptionsAt.ToList<FloatMenuOption>()));
                return false;
            }
        }

        public void TryLaunch(int destinationTile, TransportPodsArrivalAction arrivalAction)
        {
            if (!this.parent.Spawned)
            {
                Log.Error("Tried to launch " + this.parent + ", but it's unspawned.", false);
                return;
            }
            //List<CompTransporter> transportersInGroup = this.TransportersInGroup;
            //if (transportersInGroup == null)
            //{
            //    Log.Error("Tried to launch " + this.parent + ", but it's not in any group.", false);
            //    return;
            //}
            if (!this.LoadingInProgressOrReadyToLaunch)
            {
                return;
            }
            Map map = this.parent.Map;
            int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile, true, int.MaxValue);
            if (num > this.MaxLaunchDistance)
            {
                return;
            }
            this.Transporter.TryRemoveLord(map);
            int groupID = this.Transporter.groupID;
            //for (int i = 0; i < transportersInGroup.Count; i++)
            //{
            //    CompTransporter compTransporter = transportersInGroup[i];
            //    ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
            //    ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
            //    activeDropPod.Contents = new ActiveDropPodInfo();
            //    activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
            //    DropPodLeaving dropPodLeaving = (DropPodLeaving)SkyfallerMaker.MakeSkyfaller(ThingDefOf.DropPodLeaving, activeDropPod);
            //    dropPodLeaving.groupID = groupID;
            //    dropPodLeaving.destinationTile = destinationTile;
            //    dropPodLeaving.arrivalAction = arrivalAction;
            //    compTransporter.CleanUpLoadingVars(map);
            //    compTransporter.parent.Destroy(DestroyMode.Vanish);
            //    GenSpawn.Spawn(dropPodLeaving, compTransporter.parent.Position, map, WipeMode.Vanish);
            //}
            CompTransporter compTransporter = this.Transporter;
            ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
            ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
            activeDropPod.Contents = new ActiveDropPodInfo();
            activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
            DropPodLeaving dropPodLeaving = (DropPodLeaving)SkyfallerMaker.MakeSkyfaller(ThingDefOf.DropPodLeaving, activeDropPod);
            dropPodLeaving.groupID = groupID;
            dropPodLeaving.destinationTile = destinationTile;
            dropPodLeaving.arrivalAction = arrivalAction;
            compTransporter.CleanUpLoadingVars(map);
            compTransporter.parent.Destroy(DestroyMode.Vanish);
            GenSpawn.Spawn(dropPodLeaving, compTransporter.parent.Position, map, WipeMode.Vanish);
            CameraJumper.TryHideWorld();
        }

        public static int MaxLaunchDistanceAtFuelLevel(float fuelLevel)
        {
            return Mathf.FloorToInt(fuelLevel / 2.25f);
        }

        public IEnumerable<FloatMenuOption> GetTeleporterFloatMenuOptionsAt(int tile)
        {
            bool anything = false;
            // TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(this.TransportersInGroup.Cast<IThingHolder>(), tile)
            if (!Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
            {
                anything = true;
                yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate ()
                {
                    this.TryLaunch(tile, new TransportPodsArrivalAction_FormCaravan());
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            /*List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
            for (int i = 0; i < worldObjects.Count; i++)
            {
                if (worldObjects[i].Tile == tile)
                {
                    foreach (FloatMenuOption o in worldObjects[i].GetTransportPodsFloatMenuOptions(this.TransportersInGroup.Cast<IThingHolder>(), this))
                    {
                        anything = true;
                        yield return o;
                    }
                }
            }*/
            // !anything && 
            if (!Find.World.Impassable(tile))
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
