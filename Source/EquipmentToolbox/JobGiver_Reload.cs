using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace EquipmentToolbox
{
    public class JobGiver_Reload : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            return 5.8f; // 0.1f below vanilla reload
        }

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!ModSettingGetter.allowEquipmentReloading) return null;
			List<CompThingAbility> compThingAbilities = AmmoUtility.FindAllCompsNeedingReload(pawn);
			if (compThingAbilities.Count == 0)
			{
				return null;
			}
			foreach (CompThingAbility compThingAbility in compThingAbilities)
            {
				List<Thing> list = AmmoUtility.FindEnoughAmmo(pawn, pawn.Position, compThingAbility);
				if (list != null)
                {
					return JobGiver_Reload.MakeReloadJob(compThingAbility, list);
				}
			}
			return null;
		}

		public static Job MakeReloadJob(CompThingAbility compThingAbility, List<Thing> chosenAmmo)
		{
			Job job = JobMaker.MakeJob(EquipmentToolboxDefOfs.EquipmentToolbox_Reload, compThingAbility.parent);
			job.targetQueueB = (from t in chosenAmmo select new LocalTargetInfo(t)).ToList<LocalTargetInfo>();
			job.count = chosenAmmo.Sum((Thing t) => t.stackCount);
			job.count = Math.Min(job.count, compThingAbility.MaxAmmoNeeded());
			return job;
		}
	}
}
