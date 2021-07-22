using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EquipmentToolbox
{
    public class Verb_LaunchThingAbilityProjectile : Verb_LaunchProjectile
    {
        public Verb_LaunchThingAbilityProjectile()
        {

        }

        public Verb_LaunchThingAbilityProjectile(VerbProperties verbProperties, Pawn pawn, VerbTracker verbTracker, CompThingAbility compThingAbility) : base()
        {
            this.verbProps = verbProperties;
            this.caster = pawn;
            this.verbTracker = verbTracker;
            this.compThingAbility = compThingAbility;
        }

        public override bool MultiSelect
        {
            get
            {
                return true;
            }
        }

        public override Texture2D UIIcon
        {
            get
            {
                return TexCommand.Attack;
            }
        }

        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false)
        {
            if (compThingAbility != null) compThingAbility.BeginTargeting();
            return base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire);
        }

        protected override bool TryCastShot()
        {
            if (compThingAbility == null || !compThingAbility.ConsumeAmmo())
            {
                CasterPawn.jobs.StopAll();
                return false;
            }
            // TODO compThingAbility.Props.cannotMiss
            return base.TryCastShot();
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            return base.ValidateTarget(target, true) && AmmoUtility.CanUseConsideringQueuedJobs(compThingAbility);
        }
        public override void OrderForceTarget(LocalTargetInfo target)
        {
            Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThingStatic, target);
            job.verbToUse = this;
            CasterPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public CompThingAbility compThingAbility;
    }
}
