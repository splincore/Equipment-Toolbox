using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace EquipmentToolbox
{
    public class JobGiver_AIUseEquipmentAbility : JobGiver_AIFightEnemy
    {
        protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
        {
            Thing enemyTarget = pawn.mindState.enemyTarget;
            if (tmpVerb == null)
            {
                dest = IntVec3.Invalid;
                return false;
            }
            return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                target = enemyTarget,
                verb = tmpVerb,
                maxRangeFromTarget = tmpVerb.verbProps.range,
                wantCoverFromTarget = tmpVerb.verbProps.range > 5f
            }, out dest);
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            UpdateEnemyTarget(pawn);
            Thing enemyTarget = pawn.mindState.enemyTarget;
            if (enemyTarget == null)
            {
                return null;
            }
            Pawn pawn2 = enemyTarget as Pawn;
            if (pawn2 != null && pawn2.IsInvisible())
            {
                return null;
            }
            List<Verb> possibleAbilities = GetUseableEquipmentAbilitiesOnTarget(pawn, enemyTarget);
            if (possibleAbilities.Count > 0)
            {
                tmpVerb = possibleAbilities.First();
                if (!TryFindShootingPosition(pawn, out IntVec3 intVec)) return null;
                if (intVec == pawn.Position)
                {
                    Job jobAbility = JobMaker.MakeJob(JobDefOf.UseVerbOnThingStatic, enemyTarget);
                    jobAbility.verbToUse = possibleAbilities.RandomElement();
                    return jobAbility;
                }
                else
                {
                    Job job = JobMaker.MakeJob(JobDefOf.Goto, intVec);
                    job.expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange;
                    job.checkOverrideOnExpire = true;
                    return job;
                }
            }
            return null;
        }

        public List<Verb> GetUseableEquipmentAbilitiesOnTarget(Pawn pawn, Thing target)
        {
            List<Verb> possibleAbilities = new List<Verb>();
            foreach (ThingWithComps thingWithComps in pawn.equipment.AllEquipmentListForReading)
            {
                foreach (ThingComp thingComp in thingWithComps.AllComps.FindAll(c => c is CompThingAbility compThingAbility && compThingAbility.CanBeUsedByAIOnTargetRightNow(target)))
                {
                    possibleAbilities.Add(((CompThingAbility)thingComp).Verb);
                }
            }
            foreach (ThingWithComps thingWithComps in pawn.apparel.WornApparel)
            {
                foreach (ThingComp thingComp in thingWithComps.AllComps.FindAll(c => c is CompThingAbility compThingAbility && compThingAbility.CanBeUsedByAIOnTargetRightNow(target)))
                {
                    possibleAbilities.Add(((CompThingAbility)thingComp).Verb);
                }
            }
            return possibleAbilities;
        }

        Verb tmpVerb = null;
    }
}
