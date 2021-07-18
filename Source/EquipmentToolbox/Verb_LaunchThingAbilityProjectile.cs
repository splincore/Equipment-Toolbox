using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EquipmentToolbox
{
    public class Verb_LaunchThingAbilityProjectile : Verb_LaunchProjectile
    {
        public Verb_LaunchThingAbilityProjectile(VerbProperties verbProperties, Pawn pawn, VerbTracker verbTracker, CompThingAbility compThingAbility)
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

        protected override bool TryCastShot()
        {
            if (!compThingAbility.ConsumeAmmo())
            {
                CasterPawn.jobs.StopAll();
                return false;
            }
            return base.TryCastShot();
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            return base.ValidateTarget(target, true) && compThingAbility.HasAmmoRemaining;
        }
        public override void OrderForceTarget(LocalTargetInfo target)
        {
            Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThingStatic, target);
            job.verbToUse = this;
            CasterPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
        }

        public CompThingAbility compThingAbility;
    }
}
