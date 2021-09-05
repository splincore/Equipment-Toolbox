using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace EquipmentToolbox
{
    public class Comp_Transformable : CompUseEffect
    {
		public CompProperties_Transformable Props
		{
			get
			{
				return props as CompProperties_Transformable;
			}
		}

        public string UniqueCompID
        {
            get
            {
                return parent.GetUniqueLoadID() + "_CompTransformable_" + Props.uniqueCompID;
            }
        }

        public float TransformTicks
        {
            get
            {
                return GenTicks.SecondsToTicks(Props.transformTime);
            }
        }

        public Pawn Wearer
        {
            get
            {
                if (parent.holdingOwner != null && parent.holdingOwner.Owner != null && parent.holdingOwner.Owner.ParentHolder != null && parent.holdingOwner.Owner.ParentHolder is Pawn pawn) return pawn;
                return null;
            }
        }

        public Texture2D IconTransform
        {
            get
            {
                var resolvedTexture = TexCommand.GatherSpotActive;
                if (!Props.TransformIcon.NullOrEmpty()) resolvedTexture = ContentFinder<Texture2D>.Get(Props.TransformIcon, true);
                return resolvedTexture;
            }
        }

        public void Transform()
        {

        }

        public void MakeTransformJob()
        {
            if (Props.transformTime <= 0f)
            {
                Transform();
            }
            else
            {
                Job transformJob = JobMaker.MakeJob(EquipmentToolboxDefOfs.EquipmentToolbox_TransformThing, Wearer, parent);                
                if (Wearer.jobs.TryTakeOrderedJob(transformJob))
                {
                    if (Wearer.CurJob.GetCachedDriver(Wearer) is JobDriverTransformThing jobDriverTransformThing)
                    {
                        jobDriverTransformThing.uniqueCompID = UniqueCompID;
                    }
                }
            }
        }

        public void PlaySound(SoundDef soundToPlay)
        {
            if (soundToPlay == null) return;
            SoundInfo info = SoundInfo.InMap(new TargetInfo(Wearer.PositionHeld, Wearer.MapHeld, false), MaintenanceType.None);
            soundToPlay?.PlayOneShot(info);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            
            yield break;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }
    }
}
