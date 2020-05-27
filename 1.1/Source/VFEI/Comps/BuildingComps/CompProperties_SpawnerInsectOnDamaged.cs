﻿using System;
using Verse;
using RimWorld;

namespace VFEI
{
    class CompProperties_SpawnerInsectOnDamaged : CompProperties
    {
        public CompProperties_SpawnerInsectOnDamaged()
        {
            this.compClass = typeof(CompSpawnerInsectOnDamaged);
        }
        #pragma warning disable 0649
        public int level = 0;
    }
}