using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace VFEI.Comps.ItemComps
{
    class CompProperties_CustomTransporter : CompProperties_Transporter
    {
		public CompProperties_CustomTransporter()
		{
			this.compClass = typeof(CompCustomTransporter);
		}
		/* #pragma warning disable 0649
		public new float massCapacity = 150f;
		public new float restEffectiveness;
		public new bool max1PerGroup;
		public new bool canChangeAssignedThingsAfterStarting; */
	}

    [StaticConstructorOnStartup]
    class CompCustomTransporter : CompTransporter
    {
		/* public new CompProperties_Transporter Props
		{
			get
			{
				return (CompProperties_CustomTransporter)this.props;
			}
		} */

		// private static readonly Texture2D CancelLoadCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);

		private static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter", true);

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
			{
				yield return gizmo;
			}
			if (this.Shuttle != null && !this.Shuttle.ShowLoadingGizmos)
			{
				yield break;
			}
			if (this.LoadingInProgressOrReadyToLaunch)
			{
				if (this.Shuttle == null || !this.Shuttle.Autoload)
				{
					yield return new Command_Action
					{
						defaultLabel = "CommandCancelLoad".Translate(),
						defaultDesc = "CommandCancelLoadDesc".Translate(),
						icon = CancelLoadCommandTex,
						action = delegate ()
						{
							SoundDefOf.Designate_Cancel.PlayOneShotOnCamera(null);
							this.CancelLoad();
						}
					};
				}
				if (this.Props.canChangeAssignedThingsAfterStarting && (this.Shuttle == null || !this.Shuttle.Autoload))
				{
					yield return new Command_LoadToTransporter
					{
						defaultLabel = "CommandSetToLoadTransporter".Translate(),
						defaultDesc = "CommandSetToLoadTransporterDesc".Translate(),
						icon = LoadCommandTex,
						transComp = this
					};
				}
			}
			else
			{
				Command_LoadToTransporter command_LoadToTransporter = new Command_LoadToTransporter();
				if (this.Props.max1PerGroup)
				{
					if (this.Props.canChangeAssignedThingsAfterStarting)
					{
						command_LoadToTransporter.defaultLabel = "CommandSetToLoadTransporter".Translate();
						command_LoadToTransporter.defaultDesc = "CommandSetToLoadTransporterDesc".Translate();
					}
					else
					{
						command_LoadToTransporter.defaultLabel = "CommandLoadTransporterSingle".Translate();
						command_LoadToTransporter.defaultDesc = "CommandLoadTransporterSingleDesc".Translate();
					}
				}
				else
				{
					int num = 0;
					for (int i = 0; i < Find.Selector.NumSelected; i++)
					{
						Thing thing = Find.Selector.SelectedObjectsListForReading[i] as Thing;
						if (thing != null && thing.def == this.parent.def)
						{
							CompLaunchable compLaunchable = thing.TryGetComp<CompLaunchable>();
							if (compLaunchable == null || (compLaunchable.FuelingPortSource != null && compLaunchable.FuelingPortSourceHasAnyFuel))
							{
								num++;
							}
						}
					}
					command_LoadToTransporter.defaultLabel = "CommandLoadTransporter".Translate(num.ToString());
					command_LoadToTransporter.defaultDesc = "CommandLoadTransporterDesc".Translate();
				}
				command_LoadToTransporter.icon = LoadCommandTex;
				command_LoadToTransporter.transComp = this;
				CompLaunchable launchable = null; // this.Launchable;
				if (launchable != null)
				{
					if (!launchable.ConnectedToFuelingPort)
					{
						command_LoadToTransporter.Disable("CommandLoadTransporterFailNotConnectedToFuelingPort".Translate());
					}
					else if (!launchable.FuelingPortSourceHasAnyFuel)
					{
						command_LoadToTransporter.Disable("CommandLoadTransporterFailNoFuel".Translate());
					}
				}
				yield return command_LoadToTransporter;
			}
			yield break;
		}
	}
}
