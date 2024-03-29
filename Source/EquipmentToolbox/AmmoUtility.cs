﻿using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace EquipmentToolbox
{
    public static class AmmoUtility
    {
        public static bool CanUseConsideringQueuedJobs(CompThingAbility compThingAbility)
        {
            if (compThingAbility == null) return false;
            if (compThingAbility.AmmoDef == null) return true;
            if (!compThingAbility.HasAmmoRemaining) return false;
            int num = 0;
            foreach (Job job in compThingAbility.Wearer.jobs.AllJobs())
            {
                if (job.verbToUse is Verb_LaunchThingAbilityProjectile verb)
                {
                    if (verb.compThingAbility != null && verb.compThingAbility.AmmoDef == compThingAbility.AmmoDef) num++;
                }
            }
            if (num == compThingAbility.RemainingCharges) return false;
            return true;
        }

        public static List<ThingComp> FindAllCompsNeedingReload(Pawn pawn)
        {
            List<ThingComp> compsNeedingReload = new List<ThingComp>();
            if (pawn.equipment != null)
            {
                foreach (ThingWithComps thingWithComps in pawn.equipment.AllEquipmentListForReading)
                {
                    foreach (CompThingAbility compThingAbility in thingWithComps.AllComps.FindAll(c => c is CompThingAbility))
                    {
                        if (compThingAbility.NeedsReload()) compsNeedingReload.Add(compThingAbility);
                    }
                    foreach (CompTransformable compTransformable in thingWithComps.AllComps.FindAll(c => c is CompTransformable))
                    {
                        if (compTransformable.NeedsReload()) compsNeedingReload.Add(compTransformable);
                    }
                }
            }
            if (pawn.apparel != null)
            {
                foreach (ThingWithComps thingWithComps in pawn.apparel.WornApparel)
                {
                    foreach (CompThingAbility compThingAbility in thingWithComps.AllComps.FindAll(c => c is CompThingAbility))
                    {
                        if (compThingAbility.NeedsReload()) compsNeedingReload.Add(compThingAbility);
                    }
                    foreach (CompTransformable compTransformable in thingWithComps.AllComps.FindAll(c => c is CompTransformable))
                    {
                        if (compTransformable.NeedsReload()) compsNeedingReload.Add(compTransformable);
                    }
                }
            }
            return compsNeedingReload;
        }

        public static List<Thing> FindEnoughAmmo(Pawn pawn, IntVec3 rootCell, CompThingAbility compThingAbility)
        {
            IntRange desiredQuantity = new IntRange(compThingAbility.MinAmmoNeeded(), compThingAbility.MaxAmmoNeeded());
            return RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, rootCell, desiredQuantity, (Thing t) => t.def == compThingAbility.AmmoDef);
        }

        public static List<Thing> FindEnoughAmmo(Pawn pawn, IntVec3 rootCell, CompTransformable compTransformable)
        {
            IntRange desiredQuantity = new IntRange(compTransformable.MinAmmoNeeded(), compTransformable.MaxAmmoNeeded());
            return RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, rootCell, desiredQuantity, (Thing t) => t.def == compTransformable.AmmoDef);
        }
    }
}
