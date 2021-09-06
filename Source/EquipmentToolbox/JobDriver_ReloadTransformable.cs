using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace EquipmentToolbox
{
    public class JobDriver_ReloadTransformable : JobDriver
    {
        private ThingWithComps Gear
        {
            get
            {
                return job.GetTarget(TargetIndex.A).Thing as ThingWithComps;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job, 1, -1, null);
            compTransformable = (CompTransformable)Gear.AllComps.Find(c => c is CompTransformable comp && comp.AmmoDef == job.targetQueueB.First().Thing.def && comp.MinAmmoNeeded() <= job.count && job.count <= comp.MaxAmmoNeeded());
            if (compTransformable == null) return false;
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => compTransformable == null);
            this.FailOn(() => compTransformable.Wearer != pawn);
            this.FailOn(() => !compTransformable.NeedsReload());
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            Toil getNextIngredient = Toils_General.Label();
            yield return getNextIngredient;
            foreach (Toil toil in ReloadAsMuchAsPossible())
            {
                yield return toil;
            }
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B, true);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(TargetIndex.B).NullOrEmpty<LocalTargetInfo>());
            foreach (Toil toil2 in this.ReloadAsMuchAsPossible())
            {
                yield return toil2;
            }
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Thing carriedThing = pawn.carryTracker.CarriedThing;
                    if (carriedThing != null && !carriedThing.Destroyed)
                    {
                        pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Thing thing, null);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield break;
        }

        private IEnumerable<Toil> ReloadAsMuchAsPossible()
        {
            Toil done = Toils_General.Label();
            yield return Toils_Jump.JumpIf(done, () => pawn.carryTracker.CarriedThing == null || pawn.carryTracker.CarriedThing.stackCount < compTransformable.MinAmmoNeeded());
            yield return Toils_General.Wait(compTransformable.Props.baseReloadTicks, TargetIndex.None).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Thing carriedThing = this.pawn.carryTracker.CarriedThing;
                    compTransformable.ReloadFrom(carriedThing);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return done;
            yield break;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<CompTransformable>(ref compTransformable, "compThingAbility", null, false);
        }

        CompTransformable compTransformable = null;
    }
}
