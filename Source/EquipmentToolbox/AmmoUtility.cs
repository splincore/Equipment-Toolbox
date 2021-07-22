using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
