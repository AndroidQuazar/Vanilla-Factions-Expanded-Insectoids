using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using Verse;
using System.Linq;


namespace InsectoidBioengineering
{
    [StaticConstructorOnStartup]
    public class Command_SetFirstGenomeList : Command
    {

        public Map map;
        public Building_BioengineeringIncubator building;
        public List<Thing> genome;



        public Command_SetFirstGenomeList()
        {
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_BioengineeringIncubator;
                if (building != null)
                {
                    

                    if (building.theFirstGenomeIAmGoingToInsert == "None")
                    {
                       
                        icon = ContentFinder<Texture2D>.Get("Things/Item/VFEI_NoGenome", true);
                        defaultLabel = "VFEI_InsertFirstGenomeNone".Translate();
                    }
                    else
                    if (building.theFirstGenomeIAmGoingToInsert == "VFEI_RoyalGenome")
                    {
                      
                        icon = ContentFinder<Texture2D>.Get("Things/Item/VFEI_RoyalGenome", true);
                        defaultLabel = "VFEI_InsertFirstGenomeRoyal".Translate();
                    }
                    else if (building.theFirstGenomeIAmGoingToInsert == "VFEI_WarriorGenome")
                    {
                        icon = ContentFinder<Texture2D>.Get("Things/Item/VFEI_WarriorGenome", true);
                        defaultLabel = "VFEI_InsertFirstGenomeWarrior".Translate();
                    }
                    else if (building.theFirstGenomeIAmGoingToInsert == "VFEI_DroneGenome")
                    {
                        icon = ContentFinder<Texture2D>.Get("Things/Item/VFEI_DroneGenome", true);
                        defaultLabel = "VFEI_InsertFirstGenomeDrone".Translate();
                    }
                }
            }
            


        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            list.Add(new FloatMenuOption("VFEI_None".Translate(), delegate
            {
                Building_BioengineeringIncubator building = (Building_BioengineeringIncubator)this.building;
                building.theFirstGenomeIAmGoingToInsert = "None";
                
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("VFEI_RoyalGenome".Translate(), delegate
            {
                genome = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("VFEI_RoyalGenome", true));
                if (genome.Count >0)
                {
                   
                    this.TryInsertFirstGenome();
                } else
                {
                    Messages.Message("VFEI_NoGeneticMaterial".Translate(), null, MessageTypeDefOf.NegativeEvent, true);
                }

            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("VFEI_WarriorGenome".Translate(), delegate
            {
                genome = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("VFEI_WarriorGenome", true));
                if (genome.Count > 0)
                {
                   
                    this.TryInsertFirstGenome();
                }
                else
                {
                    Messages.Message("VFEI_NoGeneticMaterial".Translate(), null, MessageTypeDefOf.NegativeEvent, true);
                }

            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("VFEI_DroneGenome".Translate(), delegate
            {
                genome = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("VFEI_DroneGenome", true));
                if (genome.Count > 0)
                {
                    
                    this.TryInsertFirstGenome();
                }
                else
                {
                    Messages.Message("VFEI_NoGeneticMaterial".Translate(), null, MessageTypeDefOf.NegativeEvent, true);
                }

            }, MenuOptionPriority.Default, null, null, 29f, null, null));


            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void TryInsertFirstGenome()
        {
            Building_BioengineeringIncubator building = (Building_BioengineeringIncubator)this.building;
            //Log.Message("Inserting "+ genome.RandomElement().def.defName +" on "+building.ToString());
            building.ExpectingFirstGenome = true;
            building.theFirstGenomeIAmGoingToInsert = genome.RandomElement().def.defName;
        }




    }


    [StaticConstructorOnStartup]
    public class Command_SetSecondGenomeList : Command
    {

        public Map map;
        public Building_BioengineeringIncubator building;
        public List<Thing> genome;



        public Command_SetSecondGenomeList()
        {
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_BioengineeringIncubator;
                if (building != null)
                {


                    if (building.theSecondGenomeIAmGoingToInsert == "None")
                    {

                        icon = ContentFinder<Texture2D>.Get("Things/Item/VFEI_NoGenome", true);
                        defaultLabel = "VFEI_InsertSecondGenomeNone".Translate();
                    }
                    else
                    if (building.theSecondGenomeIAmGoingToInsert == "VFEI_RoyalGenome")
                    {

                        icon = ContentFinder<Texture2D>.Get("Things/Item/VFEI_RoyalGenome", true);
                        defaultLabel = "VFEI_InsertSecondGenomeRoyal".Translate();
                    }
                    else if (building.theSecondGenomeIAmGoingToInsert == "VFEI_WarriorGenome")
                    {
                        icon = ContentFinder<Texture2D>.Get("Things/Item/VFEI_WarriorGenome", true);
                        defaultLabel = "VFEI_InsertSecondGenomeWarrior".Translate();
                    }
                    else if (building.theSecondGenomeIAmGoingToInsert == "VFEI_DroneGenome")
                    {
                        icon = ContentFinder<Texture2D>.Get("Things/Item/VFEI_DroneGenome", true);
                        defaultLabel = "VFEI_InsertSecondGenomeDrone".Translate();
                    }
                }
            }



        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            list.Add(new FloatMenuOption("VFEI_None".Translate(), delegate
            {
                Building_BioengineeringIncubator building = (Building_BioengineeringIncubator)this.building;
                building.theSecondGenomeIAmGoingToInsert = "None";

            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("VFEI_RoyalGenome".Translate(), delegate
            {
                genome = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("VFEI_RoyalGenome", true));
                if (genome.Count > 0)
                {

                    this.TryInsertSecondGenome();
                }
                else
                {
                    Messages.Message("VFEI_NoGeneticMaterial".Translate(), null, MessageTypeDefOf.NegativeEvent, true);
                }

            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("VFEI_WarriorGenome".Translate(), delegate
            {
                genome = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("VFEI_WarriorGenome", true));
                if (genome.Count > 0)
                {

                    this.TryInsertSecondGenome();
                }
                else
                {
                    Messages.Message("VFEI_NoGeneticMaterial".Translate(), null, MessageTypeDefOf.NegativeEvent, true);
                }

            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("VFEI_DroneGenome".Translate(), delegate
            {
                genome = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("VFEI_DroneGenome", true));
                if (genome.Count > 0)
                {

                    this.TryInsertSecondGenome();
                }
                else
                {
                    Messages.Message("VFEI_NoGeneticMaterial".Translate(), null, MessageTypeDefOf.NegativeEvent, true);
                }

            }, MenuOptionPriority.Default, null, null, 29f, null, null));


            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void TryInsertSecondGenome()
        {
            Building_BioengineeringIncubator building = (Building_BioengineeringIncubator)this.building;
            //Log.Message("Inserting " + genome.RandomElement().def.defName + " on " + building.ToString());
            building.ExpectingSecondGenome = true;
            building.theSecondGenomeIAmGoingToInsert = genome.RandomElement().def.defName;
        }




    }

    [StaticConstructorOnStartup]
    public class Command_SetThirdGenomeList : Command
    {

        public Map map;
        public Building_BioengineeringIncubator building;
        public List<Thing> genome;



        public Command_SetThirdGenomeList()
        {
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_BioengineeringIncubator;
                if (building != null)
                {


                    if (building.theThirdGenomeIAmGoingToInsert == "None")
                    {

                        icon = ContentFinder<Texture2D>.Get("Things/Item/VFEI_NoGenome", true);
                        defaultLabel = "VFEI_InsertThirdGenomeNone".Translate();
                    }
                    else
                    if (building.theThirdGenomeIAmGoingToInsert == "VFEI_RoyalGenome")
                    {

                        icon = ContentFinder<Texture2D>.Get("Things/Item/VFEI_RoyalGenome", true);
                        defaultLabel = "VFEI_InsertThirdGenomeRoyal".Translate();
                    }
                    else if (building.theThirdGenomeIAmGoingToInsert == "VFEI_WarriorGenome")
                    {
                        icon = ContentFinder<Texture2D>.Get("Things/Item/VFEI_WarriorGenome", true);
                        defaultLabel = "VFEI_InsertThirdGenomeWarrior".Translate();
                    }
                    else if (building.theThirdGenomeIAmGoingToInsert == "VFEI_DroneGenome")
                    {
                        icon = ContentFinder<Texture2D>.Get("Things/Item/VFEI_DroneGenome", true);
                        defaultLabel = "VFEI_InsertThirdGenomeDrone".Translate();
                    }
                }
            }



        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            list.Add(new FloatMenuOption("VFEI_None".Translate(), delegate
            {
                Building_BioengineeringIncubator building = (Building_BioengineeringIncubator)this.building;
                building.theThirdGenomeIAmGoingToInsert = "None";

            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("VFEI_RoyalGenome".Translate(), delegate
            {
                genome = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("VFEI_RoyalGenome", true));
                if (genome.Count > 0)
                {

                    this.TryInsertThirdGenome();
                }
                else
                {
                    Messages.Message("VFEI_NoGeneticMaterial".Translate(), null, MessageTypeDefOf.NegativeEvent, true);
                }

            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("VFEI_WarriorGenome".Translate(), delegate
            {
                genome = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("VFEI_WarriorGenome", true));
                if (genome.Count > 0)
                {

                    this.TryInsertThirdGenome();
                }
                else
                {
                    Messages.Message("VFEI_NoGeneticMaterial".Translate(), null, MessageTypeDefOf.NegativeEvent, true);
                }

            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("VFEI_DroneGenome".Translate(), delegate
            {
                genome = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("VFEI_DroneGenome", true));
                if (genome.Count > 0)
                {

                    this.TryInsertThirdGenome();
                }
                else
                {
                    Messages.Message("VFEI_NoGeneticMaterial".Translate(), null, MessageTypeDefOf.NegativeEvent, true);
                }

            }, MenuOptionPriority.Default, null, null, 29f, null, null));


            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void TryInsertThirdGenome()
        {
            Building_BioengineeringIncubator building = (Building_BioengineeringIncubator)this.building;
            //Log.Message("Inserting " + genome.RandomElement().def.defName + " on " + building.ToString());
            building.ExpectingThirdGenome = true;
            building.theThirdGenomeIAmGoingToInsert = genome.RandomElement().def.defName;
        }




    }


}

