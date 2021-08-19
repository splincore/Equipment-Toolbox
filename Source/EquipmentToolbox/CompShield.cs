using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public bool CanDrawNow(bool isDrafted)
        {
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
                Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(rot.AsInt, Vector3.up), material, 0);
            }
            else if (Props.graphicData == null)
            {
                Material material = Props.graphicData.Graphic.MatAt(rot);
                Vector3 drawLoc = drawPos + Props.graphicData.DrawOffsetForRot(rot);
                Mesh mesh = Props.graphicData.Graphic.MeshAt(rot);
                Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(rot.AsInt, Vector3.up), material, 0);
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

        public bool BlockDamage(ref DamageInfo damageInfo, Pawn pawn)
        {
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

            float blockChance = 0f;
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
            if (!Rand.Chance(blockChance)) return false; // cannot block: random block chance

            if (Props.useFatigueSystem && Props.maxFatigue > 0f && Props.ifBlockedDamageToFatigueFactor > 0f && Props.fatigueResetAfterTicks > 0f)
            {
                if (Find.TickManager.TicksGame >= (lastTakenDamageTick + Props.fatigueResetAfterTicks)) currentFatigue = 0;
                if ((damageInfo.Amount * Props.ifBlockedDamageToFatigueFactor) + currentFatigue > Props.maxFatigue) return false;  // cannot block: would get over max fatigue
                currentFatigue += (damageInfo.Amount * Props.ifBlockedDamageToFatigueFactor);
                lastTakenDamageTick = Find.TickManager.TicksGame;
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
                damageInfo.SetAmount(Props.ifBlockedDamageToPawnFactor * damageInfo.Amount);
                return false; // block is successful, but pawn still takes reduced damage, so return false
            }

            return true;  //  return true: block successful
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref currentFatigue, "currentFatigue", 0f, false);
            Scribe_Values.Look<int>(ref lastTakenDamageTick, "lastTakenDamageTick", 0, false);
        }

        private float currentFatigue = 0f;
        private int lastTakenDamageTick = 0;
    }
}
