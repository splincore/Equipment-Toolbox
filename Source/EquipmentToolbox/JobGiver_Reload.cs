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
			List<ThingComp> compsNeedingReload = AmmoUtility.FindAllCompsNeedingReload(pawn);
			if (compsNeedingReload.Count == 0)
			{
				return null;
			}
			foreach (CompThingAbility compThingAbility in compsNeedingReload.FindAll(c => c is CompThingAbility))
			{
				List<Thing> list = AmmoUtility.FindEnoughAmmo(pawn, pawn.Position, compThingAbility);
				if (list != null)
				{
					return JobGiver_Reload.MakeReloadJob(compThingAbility, list);
				}
			}
			foreach (CompTransformable compTransformable in compsNeedingReload.FindAll(c => c is CompTransformable))
			{
				List<Thing> list = AmmoUtility.FindEnoughAmmo(pawn, pawn.Position, compTransformable);
				if (list != null)
				{
					return JobGiver_Reload.MakeReloadJob(compTransformable, list);
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

		public static Job MakeReloadJob(CompTransformable compTransformable, List<Thing> chosenAmmo)
		{
			Job job = JobMaker.MakeJob(EquipmentToolboxDefOfs.EquipmentToolbox_ReloadTransformable, compTransformable.parent);
			job.targetQueueB = (from t in chosenAmmo select new LocalTargetInfo(t)).ToList<LocalTargetInfo>();
			job.count = chosenAmmo.Sum((Thing t) => t.stackCount);
			job.count = Math.Min(job.count, compTransformable.MaxAmmoNeeded());
			return job;
		}
	}
}
