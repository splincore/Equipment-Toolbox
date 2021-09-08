using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace EquipmentToolbox
{
    public class JobDriverTransformThing : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            ThingWithComps thingToTransform = (ThingWithComps)TargetB;
            if (thingToTransform.AllComps.Find(c => c is CompTransformable tmp && tmp.transformationPending) is CompTransformable compTransformable) uniqueCompID = compTransformable.UniqueCompID;
            this.FailOn(() => uniqueCompID.NullOrEmpty());
            job.count = 1;

            Toil transformThingPreparations = new Toil();
            transformThingPreparations.FailOn(() => totalNeededWork < 0);
            transformThingPreparations.initAction = delegate ()
            {
                GetActor().pather.StopDead();                
                if (thingToTransform != null && thingToTransform.AllComps.Find(c => c is CompTransformable comp1 && comp1.UniqueCompID == uniqueCompID) is CompTransformable tmpCompTransformable)
                {
                    totalNeededWork = tmpCompTransformable.TransformTicks;
                    workLeft = totalNeededWork;
                }
                else
                {
                    totalNeededWork = -1;
                }
            };
            transformThingPreparations.tickAction = delegate ()
            {
                workLeft--;
                if (workLeft <= 0f)
                {
                    transformThingPreparations.actor.jobs.curDriver.ReadyForNextToil();
                }
            };
            transformThingPreparations.defaultCompleteMode = ToilCompleteMode.Never;
            transformThingPreparations.WithProgressBar(TargetIndex.A, () => 1f - this.workLeft / this.totalNeededWork, false, -0.5f);
            yield return transformThingPreparations;

            Toil transformThing = new Toil();
            transformThing.FailOn(() => totalNeededWork < 0);
            transformThing.initAction = delegate ()
            {
                if (thingToTransform != null && thingToTransform.AllComps.Find(c => c is CompTransformable comp1 && comp1.UniqueCompID == uniqueCompID) is CompTransformable comp_Transformable)
                {
                    comp_Transformable.Transform();
                }
            };
            yield return transformThing;

            yield break;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref workLeft, "workLeft", 0f, false);
            Scribe_Values.Look<float>(ref totalNeededWork, "totalNeededWork", 0f, false);
            Scribe_Values.Look<string>(ref uniqueCompID, "uniqueCompID", "", false);
        }

        private float workLeft;
        private float totalNeededWork;
        public string uniqueCompID;
    }
}
