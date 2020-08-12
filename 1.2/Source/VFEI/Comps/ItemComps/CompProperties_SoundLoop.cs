using System;
using RimWorld;
using Verse;
using Verse.Sound;

namespace VFEI
{
    public class CompProperties_SoundLoop : CompProperties
    {
        public CompProperties_SoundLoop()
        {
            this.compClass = typeof(CompSoundLoop);
        }

        public SoundDef soundToPlay;
    }

    public class CompSoundLoop : ThingComp
    {
        public CompProperties_SoundLoop Props
        {
            get
            {
                return (CompProperties_SoundLoop)this.props;
            }
        }

        private Sustainer sustainer = null;
        
        public override void CompTick()
        {
            if (sustainer == null && this.parent.ParentHolder != null && this.parent.ParentHolder is Pawn p)
            {
                Log.Message(p.Label);
                SoundInfo info = SoundInfo.InMap(new TargetInfo(p.Position, p.Map), MaintenanceType.PerTick);
                sustainer = Props.soundToPlay.TrySpawnSustainer(info); ;
            }
            else
            {
                if (!sustainer.Ended)
                    sustainer.Maintain();
                else
                {
                    Log.Message("Saw regen sustainer ending");
                    sustainer.End();
                    sustainer = null;
                }
            }
        }
    }
}
