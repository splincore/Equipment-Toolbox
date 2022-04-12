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
    public class CompTransformable : ThingComp
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

        public bool CanBeUsedByAIConsideringTarget(Thing target)
        {
            if (!Props.canAiUse) return false;
            if (!CanTransformNow()) return false;

            if (Wearer.Drafted && Props.shouldAiAlwaysUseWhenDrafted) return true;
            if (!Wearer.Drafted && Props.shouldAiAlwaysUseWhenUnDrafted) return true;
            if (lastTransformationAttemptTick + Props.aiTransformCooldownTicks > Find.TickManager.TicksGame) return false;

            if (target == null || !target.Spawned || !Wearer.HostileTo(target)) return false;
            if (Props.shouldAiUseWhenTargetCloserThanCells > 0 && Wearer.PositionHeld.DistanceTo(target.Position) <= Props.shouldAiUseWhenTargetCloserThanCells && Rand.Chance(Props.commonalityOfAiUsage)) return true;
            if (Props.shouldAiUseWhenTargetFartherThanCells > 0 && Wearer.PositionHeld.DistanceTo(target.Position) >= Props.shouldAiUseWhenTargetFartherThanCells && Rand.Chance(Props.commonalityOfAiUsage)) return true;
            return false;
        }

        public bool CanTransformNow()
        {
            if (Wearer == null) return false;
            if (!HasAmmoRemaining) return false;
            if (Wearer.Drafted && !Props.displayGizmoWhileDrafted) return false;
            if (!Wearer.Drafted && !Props.displayGizmoWhileUndrafted) return false;
            return CanUseConsideringSpecialItemsNeeded() && CanUseConsideringFreeItemSlots() && CanUseConsideringAdditionalItemSlots();
        }

        public void DoTransformation()
        {
            if (GetTransformJob() is Job transformJob)
            {
                Wearer.jobs.TryTakeOrderedJob(transformJob);
            }
            else
            {
                Transform();
            }
        }

        public Job GetTransformJob()
        {
            Job transformJob = null;
            if (Props.transformTime <= 0f)
            {
                Transform();
                return transformJob;
            }
            else
            {
                foreach (CompTransformable tmp in parent.AllComps.FindAll(c => c is CompTransformable))
                {
                    tmp.transformationPending = false;
                }
                transformationPending = true;
                transformJob = JobMaker.MakeJob(EquipmentToolboxDefOfs.EquipmentToolbox_TransformThing, Wearer, parent);
            }
            return transformJob;
        }

        public void Transform()
        {
            if (!ConsumeAmmo()) return;
            lastTransformationAttemptTick = Find.TickManager.TicksGame;
            
            // creating basic new stuff
            float hitPointPercentage = ((float)parent.HitPoints) / ((float)parent.MaxHitPoints);
            ThingDef parentStuff = parent.Stuff;
            ThingWithComps thingTransformedInto;
            ThingWithComps thingSecondaryProduct = null;
            if (parentStuff == null)
            {
                if (Props.transformInto.MadeFromStuff)
                {
                    thingTransformedInto = (ThingWithComps)ThingMaker.MakeThing(Props.transformInto, GenStuff.DefaultStuffFor(Props.transformInto));
                }
                else
                {
                    thingTransformedInto = (ThingWithComps)ThingMaker.MakeThing(Props.transformInto);
                }
                if (Props.transformSecondaryProduct != null)
                {
                    if (Props.transformSecondaryProduct.MadeFromStuff)
                    {
                        thingSecondaryProduct = (ThingWithComps)ThingMaker.MakeThing(Props.transformSecondaryProduct, GenStuff.DefaultStuffFor(Props.transformSecondaryProduct));
                    }
                    else
                    {
                        thingSecondaryProduct = (ThingWithComps)ThingMaker.MakeThing(Props.transformSecondaryProduct);
                    }
                }                
            }
            else
            {
                if (Props.transformInto.MadeFromStuff)
                {
                    thingTransformedInto = (ThingWithComps)ThingMaker.MakeThing(Props.transformInto, parentStuff);
                }
                else
                {
                    thingTransformedInto = (ThingWithComps)ThingMaker.MakeThing(Props.transformInto);
                }
                if (Props.transformSecondaryProduct != null)
                {
                    if (Props.transformSecondaryProduct.MadeFromStuff)
                    {
                        thingSecondaryProduct = (ThingWithComps)ThingMaker.MakeThing(Props.transformSecondaryProduct, parentStuff);
                    }
                    else
                    {
                        thingSecondaryProduct = (ThingWithComps)ThingMaker.MakeThing(Props.transformSecondaryProduct);
                    }
                }                
            }
            thingTransformedInto.HitPoints = (int)((float)thingTransformedInto.MaxHitPoints * hitPointPercentage);
            if (thingSecondaryProduct != null) thingSecondaryProduct.HitPoints = (int)((float)thingSecondaryProduct.MaxHitPoints * hitPointPercentage);

            // moving/adjusting all comps
            foreach (ThingComp thingCompOld in parent.AllComps)
            {
                if (thingCompOld is CompThingAbility compThingAbilityOld)
                {
                    if (thingTransformedInto.AllComps.Find(x => x is CompThingAbility tmp && tmp.Props.uniqueCompID == compThingAbilityOld.Props.uniqueCompID) is CompThingAbility compThingAbilityNew)
                    {
                        compThingAbilityNew.RemainingCharges = compThingAbilityOld.RemainingCharges;
                    }
                    if (thingSecondaryProduct != null)
                    {
                        if (thingTransformedInto.AllComps.Find(x => x is CompThingAbility tmp && tmp.Props.uniqueCompID == compThingAbilityOld.Props.uniqueCompID) is CompThingAbility compThingAbilityNewSecondary)
                        {
                            compThingAbilityNewSecondary.RemainingCharges = compThingAbilityOld.RemainingCharges;
                        }
                    }
                }
                else if (thingCompOld is CompTransformable compTransformableOld)
                {
                    if (thingTransformedInto.AllComps.Find(x => x is CompTransformable tmp && tmp.Props.uniqueCompID == compTransformableOld.Props.uniqueCompID) is CompTransformable compTransformableNew)
                    {
                        compTransformableNew.RemainingCharges = compTransformableOld.RemainingCharges;
                    }
                    if (thingSecondaryProduct != null)
                    {
                        if (thingTransformedInto.AllComps.Find(x => x is CompThingAbility tmp && tmp.Props.uniqueCompID == compTransformableOld.Props.uniqueCompID) is CompThingAbility compTransformableNewSecondary)
                        {
                            compTransformableNewSecondary.RemainingCharges = compTransformableOld.RemainingCharges;
                        }
                    }
                }
                else if (thingCompOld is CompQuality compQualityOld)
                {
                    if (thingTransformedInto.AllComps.Find(x => x is CompQuality) is CompQuality compQualityNew)
                    {
                        thingTransformedInto.AllComps.Remove(compQualityNew);
                        thingTransformedInto.AllComps.Add(compQualityOld);
                    }
                    if (thingSecondaryProduct != null)
                    {
                        if (thingTransformedInto.AllComps.Find(x => x is CompQuality) is CompQuality compQualityNewSecondary)
                        {
                            thingSecondaryProduct.AllComps.Remove(compQualityNewSecondary);
                            thingSecondaryProduct.AllComps.Add(compQualityOld);
                        }
                    }
                }
                else if (thingCompOld is CompArt compArtOld)
                {
                    if (thingTransformedInto.AllComps.Find(x => x is CompArt) is CompArt compArtNew)
                    {
                        thingTransformedInto.AllComps.Remove(compArtNew);
                        thingTransformedInto.AllComps.Add(compArtOld);
                    }
                    if (thingSecondaryProduct != null)
                    {
                        if (thingTransformedInto.AllComps.Find(x => x is CompArt) is CompArt compArtNewSecondary)
                        {
                            thingSecondaryProduct.AllComps.Remove(compArtNewSecondary);
                            thingSecondaryProduct.AllComps.Add(compArtOld);
                        }
                    }
                }
                else if (thingCompOld.GetType().ToString() == "Infused.CompInfused")
                {
                    thingTransformedInto.AllComps.RemoveAll(x => x.GetType().ToString() == "Infused.CompInfused");
                    thingTransformedInto.AllComps.Add(thingCompOld);
                }
                else if (thingCompOld.GetType().ToString() == "RWBYRemnant.CompTakePhoto")
                {
                    thingTransformedInto.AllComps.RemoveAll(x => x.GetType().ToString() == "RWBYRemnant.CompTakePhoto");
                    thingTransformedInto.AllComps.Add(thingCompOld);
                }
            }

            // destroy old things
            Pawn tmpPawn = Wearer;
            if (specialEffectsUtility != null) specialEffectsUtility.DoPostTransformPreDestroyEvent(tmpPawn, parent, thingTransformedInto, thingSecondaryProduct, Props.needsItemEquipped);
            if (Props.needsItemEquipped != null && Props.comsumesItemEquipped)
            {
                if (Props.needsItemEquipped.IsWeapon)
                {
                    if (tmpPawn.equipment.AllEquipmentListForReading.Find(x => x.def.defName == Props.needsItemEquipped.defName) is ThingWithComps thingEquipment)
                    {
                        tmpPawn.equipment.Remove(thingEquipment);
                        tmpPawn.equipment.Notify_EquipmentRemoved(thingEquipment);
                        thingEquipment.Destroy();
                    }
                }
                else if (Props.needsItemEquipped.IsApparel)
                {
                    if (tmpPawn.apparel.WornApparel.Find(x => x.def.defName == Props.needsItemEquipped.defName) is Apparel thingApparel)
                    {
                        tmpPawn.apparel.Remove(thingApparel);
                        tmpPawn.apparel.Notify_ApparelRemoved(thingApparel);
                        thingApparel.Destroy();
                    }
                }
            }
            if (parent.def.IsWeapon)
            {
                tmpPawn.equipment.Remove(parent);
                tmpPawn.equipment.Notify_EquipmentRemoved(parent);
            }
            else if (parent.def.IsApparel)
            {
                tmpPawn.apparel.Remove((Apparel)parent);
                tmpPawn.apparel.Notify_ApparelRemoved((Apparel)parent);                
            }
            parent.Destroy();

            // add new things
            foreach (ThingComp thingComp in thingTransformedInto.AllComps)
            {
                if (thingComp is CompTransformable compTransformable) compTransformable.lastTransformationAttemptTick = lastTransformationAttemptTick;
            }
            if (thingTransformedInto.def.IsWeapon)
            {
                tmpPawn.equipment.AddEquipment(thingTransformedInto);
                tmpPawn.equipment.Notify_EquipmentAdded(thingTransformedInto);
            }
            else if (thingTransformedInto.def.IsApparel)
            {
                tmpPawn.apparel.Wear((Apparel)thingTransformedInto);
            }
            if (thingSecondaryProduct != null)
            {
                foreach (ThingComp thingComp in thingSecondaryProduct.AllComps)
                {
                    if (thingComp is CompTransformable compTransformable) compTransformable.lastTransformationAttemptTick = lastTransformationAttemptTick;
                }
                if (thingSecondaryProduct.def.IsWeapon)
                {
                    tmpPawn.equipment.AddEquipment(thingSecondaryProduct);
                    tmpPawn.equipment.Notify_EquipmentAdded(thingSecondaryProduct);
                }
                else if (thingSecondaryProduct.def.IsApparel)
                {
                    tmpPawn.apparel.Wear((Apparel)thingTransformedInto);
                }
            }

            if (Props.transformSound != null)
            {
                Props.transformSound.PlayOneShot(new TargetInfo(tmpPawn.PositionHeld, tmpPawn.MapHeld, false));
            }

            // compatibility with PsiTech weapon infusion, if the Mod is not running nothing happens here, if PsiTech changes any names this will break
            if (thingTransformedInto.def.IsWeapon && LoadedModManager.RunningMods.Any(m => m.Name == "PsiTech")) {
                Type typeExtensionMethods = Type.GetType("PsiTech.Utility.ExtensionMethods, PsiTech");
                MethodInfo methodInfo = typeExtensionMethods.GetMethod("PsiEquipmentTracker", new Type[] { typeof(Thing) });
                object objectPsiTechEquipmentTracker = methodInfo.Invoke(parent, new object[] { parent });
                Type typePsiTechEquipmentTracker = Type.GetType("PsiTech.Misc.PsiTechEquipmentTracker, PsiTech");
                FieldInfo fieldInfo = typePsiTechEquipmentTracker.GetField("IsPsychic");
                if (typePsiTechEquipmentTracker.GetField("IsPsychic").GetValue(objectPsiTechEquipmentTracker) is bool IsPsychic && IsPsychic)
                {
                    objectPsiTechEquipmentTracker = methodInfo.Invoke(thingTransformedInto, new object[] { thingTransformedInto });
                    typePsiTechEquipmentTracker.GetField("IsPsychic").SetValue(objectPsiTechEquipmentTracker, true);
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Wearer == null || Props.transformInto == null) yield break;
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
                action = DoTransformation
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
            else if (!CanUseConsideringSpecialItemsNeeded())
            {
                command_Transformable.Disable("EquipmentToolboxNoSpecialItemEquipped".Translate(Wearer.LabelShort, parent.Label, Props.needsItemEquipped.label));
            }
            else if (!CanUseConsideringFreeItemSlots())
            {
                command_Transformable.Disable("EquipmentToolboxNoFreeItemSlotForTransformation".Translate(Wearer.LabelShort, parent.Label, Props.transformInto.label));
            }
            else if (!CanUseConsideringAdditionalItemSlots())
            {
                command_Transformable.Disable("EquipmentToolboxNoFreeItemSlotForAdditionalProduct".Translate(Wearer.LabelShort, parent.Label, Props.transformInto.label));
            }
            return command_Transformable;
        }

        public bool CanUseConsideringSpecialItemsNeeded()
        {
            if (Props.needsItemEquipped == null) return true;
            if (Props.needsItemEquipped.IsWeapon)
            {
                if (Wearer.equipment.AllEquipmentListForReading.Any(x => x.def.defName == Props.needsItemEquipped.defName)) return true;
            }
            else if (Props.needsItemEquipped.IsApparel)
            {
                if (Wearer.apparel.WornApparel.Any(x => x.def.defName == Props.needsItemEquipped.defName)) return true;
            }
            return false;
        }

        public bool CanUseConsideringFreeItemSlots()
        {
            if (Props.transformInto.IsWeapon)
            {
                if (Props.transformInto.equipmentType == EquipmentType.None || Wearer.equipment.Primary == null || Wearer.equipment.Primary == parent) return true;
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

        public bool CanUseConsideringAdditionalItemSlots()
        {
            if (Props.transformSecondaryProduct == null) return true;
            if (Props.transformSecondaryProduct.IsWeapon)
            {
                if (Props.transformSecondaryProduct.equipmentType == EquipmentType.None || !Props.transformInto.IsWeapon || Props.transformInto.equipmentType == EquipmentType.None) return true;
            }
            else if (Props.transformSecondaryProduct.IsApparel)
            { 
                if (Props.transformInto.IsWeapon)
                {
                    if (Wearer.apparel.CanWearWithoutDroppingAnything(Props.transformSecondaryProduct)) return true;
                    if (parent.def.IsApparel)
                    {
                        if (parent.def.apparel.layers == Props.transformSecondaryProduct.apparel.layers && parent.def.apparel.GetInterferingBodyPartGroups(Wearer.RaceProps.body) == Props.transformSecondaryProduct.apparel.GetInterferingBodyPartGroups(Wearer.RaceProps.body)) return true;
                    }
                }
                if (Props.transformInto.IsApparel)
                {
                    if (Props.transformInto.apparel.layers == Props.transformSecondaryProduct.apparel.layers && Props.transformInto.apparel.GetInterferingBodyPartGroups(Wearer.RaceProps.body) == Props.transformSecondaryProduct.apparel.GetInterferingBodyPartGroups(Wearer.RaceProps.body)) return false;
                    if (Wearer.apparel.CanWearWithoutDroppingAnything(Props.transformSecondaryProduct)) return true;
                }
            }
            return false;
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            IEnumerable<StatDrawEntry> enumerable = base.SpecialDisplayStats();
            if (enumerable != null)
            {
                foreach (StatDrawEntry statDrawEntry in enumerable)
                {
                    yield return statDrawEntry;
                }
            }
            StatCategoryDef statCategoryDef = StatCategoryDefOf.Basics;
            if (parent.def.IsApparel) statCategoryDef = StatCategoryDefOf.Apparel;
            if (parent.def.IsWeapon) statCategoryDef = StatCategoryDefOf.Weapon;
            string ammoName = "Stat_AmmunitionNeeded_None".Translate();
            if (AmmoDef != null) ammoName = AmmoDef.label;
            yield return new StatDrawEntry(statCategoryDef, "Stat_Thing_ReloadChargesRemaining_Name".Translate(Props.ChargeNounArgument), LabelRemaining, "Stat_AmmunitionNeededTransform_Desc".Translate(Props.abilityLabel, ammoName), 2747, null, null, false);
            yield break;
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

        public override void PostPostMake()
        {
            base.PostPostMake();
            if (Props.postTransformClass != null)
            {
                specialEffectsUtility = (SpecialEffectsUtility)Activator.CreateInstance(Props.postTransformClass);
            }
            if (Props.spawnWithFullAmmo)
            {
                remainingCharges = MaxCharges;
            }
            else
            {
                remainingCharges = 0;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref remainingCharges, "remainingCharges_CompTransformable_" + Props.uniqueCompID, 0, false);
            Scribe_Values.Look<int>(ref lastTransformationAttemptTick, "lastTransformationAttemptTick", 0, false);
            if (specialEffectsUtility == null && Props.postTransformClass != null)
            {
                specialEffectsUtility = (SpecialEffectsUtility)Activator.CreateInstance(Props.postTransformClass);
            }
        }

        private int remainingCharges;
        private int lastTransformationAttemptTick = 0;
        public bool transformationPending = false;
        public SpecialEffectsUtility specialEffectsUtility = null;
    }
}
