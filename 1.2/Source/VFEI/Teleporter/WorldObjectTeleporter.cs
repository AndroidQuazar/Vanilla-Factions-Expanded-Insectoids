using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld.Planet;

namespace VFEI
{
    [StaticConstructorOnStartup]
    class WorldObjectTeleporter : WorldObject
    {
        private List<TeleporterComp> comps = new List<TeleporterComp>();

        public virtual IEnumerable<FloatMenuOption> GetTeleporterFloatMenuOptions(IEnumerable<IThingHolder> pods, CompTeleporter representative)
        {
            for (int i = 0; i < this.comps.Count; i++)
            {
                foreach (FloatMenuOption f in this.comps[i].GetTeleporterFloatMenuOptions(pods, representative))
                {
                    yield return f;
                }
            }
            yield break;
        }

        public abstract class TeleporterComp : WorldObjectComp
        {
            public virtual IEnumerable<FloatMenuOption> GetTeleporterFloatMenuOptions(IEnumerable<IThingHolder> pods, CompTeleporter representative)
            {
                yield break;
            }
        }
    }
}
