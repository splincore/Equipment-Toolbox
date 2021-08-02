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
            if (possibleAbilityVerb == null)
            {
                dest = IntVec3.Invalid;
                return false;
            }
            CastPositionRequest castPositionRequest = new CastPositionRequest
            {
                caster = pawn,
                target = enemyTarget,
                verb = possibleAbilityVerb,
                maxRangeFromTarget = possibleAbilityVerb.verbProps.range,
                wantCoverFromTarget = possibleAbilityVerb.verbProps.range > 5f,
            };
            if (isDefensive)
            {
                castPositionRequest.maxRangeFromTarget = 9999f;
                castPositionRequest.locus = (IntVec3)pawn.mindState.duty.focus;
                castPositionRequest.maxRangeFromLocus = pawn.mindState.duty.radius;
                castPositionRequest.wantCoverFromTarget = (possibleAbilityVerb.verbProps.range > 7f);
            }
            return CastPositionFinder.TryFindCastPosition(castPositionRequest, out dest);
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
                possibleAbilityVerb = possibleAbilities.First();
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
            if (pawn.equipment != null)
            {
                foreach (ThingWithComps thingWithComps in pawn.equipment.AllEquipmentListForReading)
                {
                    foreach (ThingComp thingComp in thingWithComps.AllComps.FindAll(c => c is CompThingAbility compThingAbility && compThingAbility.CanBeUsedByAIOnTargetRightNow(target)))
                    {
                        possibleAbilities.Add(((CompThingAbility)thingComp).Verb);
                    }
                }
            }
            if (pawn.apparel != null)
            {
                foreach (ThingWithComps thingWithComps in pawn.apparel.WornApparel)
                {
                    foreach (ThingComp thingComp in thingWithComps.AllComps.FindAll(c => c is CompThingAbility compThingAbility && compThingAbility.CanBeUsedByAIOnTargetRightNow(target)))
                    {
                        possibleAbilities.Add(((CompThingAbility)thingComp).Verb);
                    }
                }
            }
            return possibleAbilities;
        }

        protected Verb possibleAbilityVerb = null;
        private bool isDefensive = false;
    }
}
