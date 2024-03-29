﻿using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EquipmentToolbox
{
    public class CompShield : ThingComp
    {
        public CompProperties_Shield Props
        {
            get
            {
                return props as CompProperties_Shield;
            }
        }

        public bool IgnoresOtherShields
        {
            get
            {
                return Props.ignoresOtherShields;
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

        public bool ShouldRenderShield
        {
            get
            {
                return shouldRenderShield;
            }
            set
            {
                shouldRenderShield = value;
            }
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
            string blockChanceMelee = Props.flatMeleeBlockChance.ToStringPercent();
            string blockChanceRanged = Props.flatRangedBlockChance.ToStringPercent();
            string blockChanceMeleeFlat = Props.flatMeleeBlockChance.ToStringPercent();
            string blockChanceRangedFlat = Props.flatRangedBlockChance.ToStringPercent();
            string blockChanceMeleeSkill = "not used";
            string blockChanceRangedSkill = "not used";
            string blockChanceMeleeQuality = "not used";
            string blockChanceRangedQuality = "not used";
            if (Wearer != null)
            {
                blockChanceMelee = GetBlockChance(Wearer, false).ToStringPercent();
                blockChanceRanged = GetBlockChance(Wearer, true).ToStringPercent();
                if (Props.curveSkillBasedMeleeBlockChance != null)
                {
                    blockChanceMeleeSkill = Props.curveSkillBasedMeleeBlockChance.Evaluate(Wearer.skills.GetSkill(Props.meleeBlockSkillToUse).Level).ToStringPercent();
                    blockChanceMeleeFlat = "not used";
                }
                if (Props.curveSkillBasedRangedBlockChance != null)
                {
                    blockChanceRangedSkill = Props.curveSkillBasedRangedBlockChance.Evaluate(Wearer.skills.GetSkill(Props.rangedBlockSkillToUse).Level).ToStringPercent();
                    blockChanceRangedFlat = "not used";
                }
            }
            if (parent.TryGetComp<CompQuality>() is CompQuality compQuality)
            {
                int qualityInt = 0;
                switch (compQuality.Quality)
                {
                    case QualityCategory.Awful:
                        qualityInt = 1;
                        break;
                    case QualityCategory.Poor:
                        qualityInt = 2;
                        break;
                    case QualityCategory.Normal:
                        qualityInt = 3;
                        break;
                    case QualityCategory.Good:
                        qualityInt = 4;
                        break;
                    case QualityCategory.Excellent:
                        qualityInt = 5;
                        break;
                    case QualityCategory.Masterwork:
                        qualityInt = 6;
                        break;
                    case QualityCategory.Legendary:
                        qualityInt = 7;
                        break;
                }
                if (Props.curveQualityBasedMeleeBlockChance != null) blockChanceMeleeQuality = Props.curveQualityBasedMeleeBlockChance.Evaluate(qualityInt).ToStringPercent();
                if (Props.curveQualityBasedRangedBlockChance != null) blockChanceRangedQuality = Props.curveQualityBasedRangedBlockChance.Evaluate(qualityInt).ToStringPercent();
            }
            if (!Props.canBlockMelee)
            {
                blockChanceMelee = "not blockable";
                blockChanceMeleeFlat = "not blockable";
                blockChanceMeleeSkill = "not blockable";
                blockChanceMeleeQuality = "not blockable";
            }
            if (!Props.canBlockRanged)
            {
                blockChanceRanged = "not blockable";
                blockChanceRangedFlat = "not blockable";
                blockChanceRangedSkill = "not blockable";
                blockChanceRangedQuality = "not blockable";
            }
            yield return new StatDrawEntry(statCategoryDef, "Stat_BlockChanceMelee_Name".Translate(), blockChanceMelee, "Stat_BlockChanceMelee_Desc".Translate(Props.meleeBlockSkillToUse.label, blockChanceMeleeFlat, blockChanceMeleeSkill, blockChanceMeleeQuality, blockChanceMelee), 2748, null, null, false);
            yield return new StatDrawEntry(statCategoryDef, "Stat_BlockChanceRanged_Name".Translate(), blockChanceRanged, "Stat_BlockChanceRanged_Desc".Translate(Props.rangedBlockSkillToUse.label, blockChanceRangedFlat, blockChanceRangedSkill, blockChanceRangedQuality, blockChanceRanged), 2748, null, null, false);
            yield break;
        }

        public bool CanDrawNow(bool isDrafted)
        {
            if (!ShouldRenderShield) return false;
            if (isDrafted && Props.drawWhenDrafted)
            {
                return true;
            }
            else if (!isDrafted && Props.drawWhenUndrafted)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DrawAt(Vector3 drawPos, Rot4 rot, bool isDrafted = false)
        {
            if (!CanDrawNow(isDrafted)) return;
            if (!isDrafted && Props.graphicDataUndrafted != null)
            {
                Material material = Props.graphicDataUndrafted.Graphic.MatAt(rot);
                Vector3 drawLoc = drawPos + Props.graphicDataUndrafted.DrawOffsetForRot(rot);
                Mesh mesh = Props.graphicDataUndrafted.Graphic.MeshAt(rot);
                Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(0, Vector3.up), material, 0);
            }
            else if (Props.graphicData != null)
            {
                Material material = Props.graphicData.Graphic.MatAt(rot);
                Vector3 drawLoc = drawPos + Props.graphicData.DrawOffsetForRot(rot);
                Mesh mesh = Props.graphicData.Graphic.MeshAt(rot);
                Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(0, Vector3.up), material, 0);
            }
        }

        public void PlayBlockSound(Pawn pawn,  bool isRanged = false)
        {
            if (isRanged)
            {
                if (Props.rangedBlockSounds.Count > 0)
                {
                    Props.rangedBlockSounds.RandomElement().PlayOneShot(new TargetInfo(pawn.PositionHeld, pawn.MapHeld, false));
                }
            }
            else
            {
                if (Props.meleeBlockSounds.Count > 0)
                {
                    Props.meleeBlockSounds.RandomElement().PlayOneShot(new TargetInfo(pawn.PositionHeld, pawn.MapHeld, false));
                }
            }
        }

        public float GetBlockChance(Pawn pawn, bool isRanged = false)
        {
            float blockChance;
            if (isRanged)
            {
                blockChance = Props.flatRangedBlockChance;
                if (Props.curveSkillBasedRangedBlockChance != null)
                {
                    blockChance = Props.curveSkillBasedRangedBlockChance.Evaluate(pawn.skills.GetSkill(Props.rangedBlockSkillToUse).Level);
                }
            }
            else
            {
                blockChance = Props.flatMeleeBlockChance;
                if (Props.curveSkillBasedMeleeBlockChance != null)
                {
                    blockChance = Props.curveSkillBasedMeleeBlockChance.Evaluate(pawn.skills.GetSkill(Props.meleeBlockSkillToUse).Level);
                }
            }
            if (parent.TryGetComp<CompQuality>() is CompQuality compQuality)
            {
                int qualityInt = 0;
                switch (compQuality.Quality)
                {
                    case QualityCategory.Awful:
                        qualityInt = 1;
                        break;
                    case QualityCategory.Poor:
                        qualityInt = 2;
                        break;
                    case QualityCategory.Normal:
                        qualityInt = 3;
                        break;
                    case QualityCategory.Good:
                        qualityInt = 4;
                        break;
                    case QualityCategory.Excellent:
                        qualityInt = 5;
                        break;
                    case QualityCategory.Masterwork:
                        qualityInt = 6;
                        break;
                    case QualityCategory.Legendary:
                        qualityInt = 7;
                        break;
                }
                if (isRanged)
                {
                    if (Props.curveQualityBasedRangedBlockChance != null)
                    {
                        blockChance = (blockChance + Props.curveQualityBasedRangedBlockChance.Evaluate(qualityInt)) / 2f;
                    }
                }
                else
                {
                    if (Props.curveQualityBasedMeleeBlockChance != null)
                    {
                        blockChance = (blockChance + Props.curveQualityBasedMeleeBlockChance.Evaluate(qualityInt)) / 2f;
                    }
                }
            }
            if (blockChance > 1f) blockChance = 1f;
            return blockChance;
        }

        public bool TryBlockDamage(ref DamageInfo damageInfo, Pawn pawn)
        {
            if (pawn.Drafted && !Props.canBlockDrafted) return false; // cannot block: drafted block not allowed
            if (!pawn.Drafted && !Props.canBlockUndrafted) return false; // cannot block: undrafted block not allowed

            bool isRanged = damageInfo.Def.isRanged || damageInfo.Instigator == null || !damageInfo.Instigator.Position.AdjacentTo8WayOrInside(pawn.Position);
            if (damageInfo.Def.isExplosive)
            {
                isRanged = Props.explosionsAreConsideredAsRanged;
            }
            if (isRanged && !Props.canBlockRanged) return false; // cannot block: ranged block not allowed
            if (!isRanged && !Props.canBlockMelee) return false; // cannot block: melee block not allowed

            if (Props.blockAngleRange <= 0f) return false; // cannot block: false configured
            if (Props.blockAngleRange < 360f && damageInfo.Angle >= 0f)
            {
                float pawnAngle = pawn.Rotation.AsInt * 90f;
                float damageIncomingAngle = (damageInfo.Angle + 180) % 360;
                if (pawn.stances.curStance is Stance_Busy stance_Busy && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
                {
                    Vector3 a;
                    if (stance_Busy.focusTarg.HasThing)
                    {
                        a = stance_Busy.focusTarg.Thing.DrawPos;
                    }
                    else
                    {
                        a = stance_Busy.focusTarg.Cell.ToVector3Shifted();
                    }
                    if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                    {
                        pawnAngle = (a - pawn.DrawPos).AngleFlat();
                    }
                }
                if (pawn.Drafted)
                {
                    pawnAngle += Props.blockAngleOffsetDrafted;
                }
                else
                {
                    pawnAngle += Props.blockAngleOffsetUndrafted;
                }
                float angleDiff = ((pawnAngle - damageIncomingAngle + 180f + 360f) % 360f) - 180f;
                if (angleDiff < -(Props.blockAngleRange / 2f) || angleDiff > (Props.blockAngleRange / 2f)) return false;  // cannot block: incoming dmg angle is out of range
            }
            
            if (!Rand.Chance(GetBlockChance(pawn, isRanged))) return false; // cannot block: random block chance

            if (Props.useFatigueSystem && Props.maxFatigue > 0f && Props.ifBlockedDamageToFatigueFactor > 0f && Props.fatigueResetAfterTicks > 0f)
            {
                if (Find.TickManager.TicksGame >= (lastTakenDamageTick + Props.fatigueResetAfterTicks)) currentFatigue = 0;
                lastTakenDamageTick = Find.TickManager.TicksGame;
                if ((damageInfo.Amount * Props.ifBlockedDamageToFatigueFactor) + currentFatigue > Props.maxFatigue) return false;  // cannot block: would get over max fatigue
                currentFatigue += (damageInfo.Amount * Props.ifBlockedDamageToFatigueFactor);
            }

            // block is successful: play sound and eventually deal damage to sield or pawn
            PlayBlockSound(pawn, isRanged);

            if (Props.ifBlockedDamageToShielFactor > 0f)
            {
                DamageInfo shieldDamageInfo = new DamageInfo(damageInfo);
                shieldDamageInfo.SetAmount(damageInfo.Amount * Props.ifBlockedDamageToShielFactor);
                parent.TakeDamage(shieldDamageInfo);
            }

            if (Props.ifBlockedDamageToPawnFactor > 0f)
            {
                ShieldUtility.unblockableDamage = new DamageInfo(damageInfo);
                ShieldUtility.unblockableDamage.SetAmount(Props.ifBlockedDamageToPawnFactor * damageInfo.Amount);
                pawn.TakeDamage(ShieldUtility.unblockableDamage);
                return true;
            }

            return true;  //  return true: block successful
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            if (Props.postBlockClass != null)
            {
                specialEffectsUtility = (SpecialEffectsUtility)Activator.CreateInstance(Props.postBlockClass);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref currentFatigue, "currentFatigue", 0f, false);
            Scribe_Values.Look<int>(ref lastTakenDamageTick, "lastTakenDamageTick", 0, false);
            Scribe_Values.Look<bool>(ref shouldRenderShield, "shouldRenderShield", true, false);
            if (specialEffectsUtility == null && Props.postBlockClass != null)
            {
                specialEffectsUtility = (SpecialEffectsUtility)Activator.CreateInstance(Props.postBlockClass);
            }
        }

        private float currentFatigue = 0f;
        private int lastTakenDamageTick = 0;
        private bool shouldRenderShield = true;
        public SpecialEffectsUtility specialEffectsUtility = null;
    }
}
