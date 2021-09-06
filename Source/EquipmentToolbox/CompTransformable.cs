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
    public class CompTransformable : CompUseEffect
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

        public bool HasAmmoRemaining
        {
            get
            {
                return remainingCharges > 0 || AmmoDef == null;
            }
        }

        public ThingDef AmmoDef
        {
            get
            {
                return Props.ammoDef;
            }
        }

        public int RemainingCharges
        {
            get
            {
                return remainingCharges;
            }
            set
            {
                if (value >= 0 && value <= MaxCharges)
                {
                    remainingCharges = value;
                }
            }
        }

        public int MaxCharges
        {
            get
            {
                return Props.maxCharges;
            }
        }

        public string LabelRemaining
        {
            get
            {
                if (AmmoDef == null)
                {
                    return string.Format("{0} / {1}", "\u221E", "\u221E");
                }
                return string.Format("{0} / {1}", RemainingCharges, MaxCharges);
            }
        }

        public Texture2D AbilityIcon
        {
            get
            {
                return ContentFinder<Texture2D>.Get(Props.abilityIcon, true);
            }
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

        public void Transform()
        {
            if (!ConsumeAmmo()) return;
            // TODO
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Wearer == null) yield break;
            bool drafted = Wearer.Drafted;
            if ((drafted && !Props.displayGizmoWhileDrafted) || (!drafted && !Props.displayGizmoWhileUndrafted))
            {
                yield break;
            }
            yield return CreateTransformCommand();           
            yield break;
        }

        private Command_Transformable CreateTransformCommand()
        {
            Command_Transformable command_Transformable = new Command_Transformable(this)
            {
                defaultLabel = Props.abilityLabel,
                defaultDesc = Props.abilityDesc,
                icon = AbilityIcon,
                iconAngle = Props.abilityIconAngle,
                iconOffset = Props.abilityIconOffset,
                action = MakeTransformJob
            };
            if (Props.abilityColor != null) command_Transformable.overrideColor = Props.abilityColor;
            if (Props.hotKey != null) command_Transformable.hotKey = Props.hotKey;
            if (!Wearer.IsColonistPlayerControlled)
            {
                command_Transformable.Disable("CannotOrderNonControlled".Translate());
            }
            else if (!HasAmmoRemaining)
            {
                command_Transformable.Disable(DisabledReason(MinAmmoNeeded(), MaxAmmoNeeded()));
            }
            else if (!CanUseConsideringFreeItemSlots())
            {
                command_Transformable.Disable("EquipmentToolboxNoFreeItemSlotForTransformation".Translate(Wearer.LabelShort, parent.Label, Props.transformInto.label));
            }
            return command_Transformable;
        }

        public bool CanUseConsideringFreeItemSlots()
        {
            if (Props.transformInto.IsWeapon)
            {
                if (Props.transformInto.equipmentType == EquipmentType.None || Wearer.equipment.Primary == parent) return true;
            }
            else if (Props.transformInto.IsApparel)
            {
                if (Wearer.apparel.CanWearWithoutDroppingAnything(Props.transformInto)) return true;
                if (parent.def.IsApparel)
                {
                    if (parent.def.apparel.layers == Props.transformInto.apparel.layers && parent.def.apparel.GetInterferingBodyPartGroups(Wearer.RaceProps.body) == Props.transformInto.apparel.GetInterferingBodyPartGroups(Wearer.RaceProps.body)) return true;
                }
            }
            return false;
        }

        public string DisabledReason(int minNeeded, int maxNeeded)
        {
            string arg;
            if (Props.ammoCountToRefill != 0)
            {
                arg = Props.ammoCountToRefill.ToString();
            }
            else
            {
                arg = ((minNeeded == maxNeeded) ? minNeeded.ToString() : string.Format("{0}-{1}", minNeeded, maxNeeded));
            }
            return "CommandReload_NoAmmo".Translate(Props.ChargeNounArgument, AmmoDef.Named("AMMO"), arg.Named("COUNT"));
        }

        public bool NeedsReload()
        {
            if (AmmoDef == null) return false;
            if (!Props.canBeReloaded) return false;
            if (Props.ammoCountToRefill != 0)
            {
                return RemainingCharges == 0;
            }
            else
            {
                return RemainingCharges < MaxCharges;
            }
        }

        public int MinAmmoNeeded()
        {
            if (Props.ammoCountToRefill != 0)
            {
                return Props.ammoCountToRefill;
            }
            return Props.ammoCountPerCharge;
        }

        public int MaxAmmoNeeded()
        {
            if (Props.ammoCountToRefill != 0)
            {
                return Props.ammoCountToRefill;
            }
            return Props.ammoCountPerCharge * (MaxCharges - RemainingCharges);
        }

        public bool ConsumeAmmo()
        {
            if (AmmoDef == null) return true;
            if (remainingCharges > 0)
            {
                remainingCharges--;
            }
            else
            {
                return false;
            }
            if (Props.destroyOnEmpty && remainingCharges == 0 && !parent.Destroyed)
            {
                parent.Destroy(DestroyMode.Vanish);
            }
            return true;
        }

        public void ReloadFrom(Thing ammo)
        {
            if (!this.NeedsReload())
            {
                return;
            }
            if (Props.ammoCountToRefill != 0)
            {
                if (ammo.stackCount < Props.ammoCountToRefill)
                {
                    return;
                }
                ammo.SplitOff(Props.ammoCountToRefill).Destroy(DestroyMode.Vanish);
                remainingCharges = MaxCharges;
            }
            else
            {
                if (ammo.stackCount < Props.ammoCountPerCharge)
                {
                    return;
                }
                int num = Mathf.Clamp(ammo.stackCount / Props.ammoCountPerCharge, 0, MaxCharges - RemainingCharges);
                ammo.SplitOff(num * Props.ammoCountPerCharge).Destroy(DestroyMode.Vanish);
                remainingCharges += num;
            }
            if (Props.soundReload != null)
            {
                Props.soundReload.PlayOneShot(new TargetInfo(Wearer.PositionHeld, Wearer.MapHeld, false));
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref remainingCharges, "remainingCharges", -999, false);
        }

        private int remainingCharges;
    }
}
