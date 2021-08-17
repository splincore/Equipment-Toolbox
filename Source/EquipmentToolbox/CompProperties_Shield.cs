using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EquipmentToolbox
{
    public class CompProperties_Shield : CompProperties
    {
        public CompProperties_Shield()
        {
            this.compClass = typeof(CompShield);
        }

        // graphics
        public GraphicData graphicData; // also uses draw size and offsets, needs <graphicClass>Graphic_Multi</graphicClass>
        public bool drawWhenDrafted = true;
        public bool drawWhenUndrafted = false;
        public GraphicData graphicDataUndrafted; // set if the undrafted graphic should be different, if null, the normal graphicData gets used when undrafted

        // block sounds: plays random sound in correspondending list
        public List<SoundDef> meleeBlockSound = new List<SoundDef>();
        public List<SoundDef> rangedBlockSound = new List<SoundDef>();

        // blocking general
        public bool canBlockMelee = true;
        public bool canBlockRanged = false;
        public float blockAngleRange = 90f; // area in front of pawn which gets covered by the shield in degrees from 0 to 360, 360 = complete area around the pawn gets covered
        public float blockAngleOffsetDrafted = 0f; // rotates covered area around the pawn clockwise, from 0 to 359, 180 would mean the shield coveres the pawns back
        public float blockAngleOffsetUndrafted = 0f; // if the pawn carries the shield on its back when undrafted you could rotate the block angle by 180 degrees

        // block chances, 0 means 0% and 1 means 100%, block chance is the average between skill block chance and quality block chance
        // a high quality shield can compensate an unskilled pawn, a low quality shield can hinder a skilled pawn
        public SimpleCurve curveSkillBasedMeleeBlockChance; // has value <points> which is a list with pairs <li>(skill level, block chance)</li> skill level is from 0 to 20
        public SimpleCurve curveSkillBasedRangedBlockChance; // SimpleCurve needs only 1 entry to work but that would mean a flat block chance, more entries in the list means a more precise skill based block chance
        public SkillDef meleeBlockSkillToUse = SkillDefOf.Melee;
        public SkillDef rangedBlockSkillToUse = SkillDefOf.Shooting;
        public SimpleCurve curveQualityBasedMeleeBlockChance; // same as skill based curve but this time the list needs <li>(quality value, block chance)</li> lowest quality value is 1 for awful and highest is 7 for legendary
        public SimpleCurve curveQualityBasedRangedBlockChance; // if quality curves are not set, quality of the shield is not taken into account when blocking
        public float flatMeleeBlockChance = 0.5f; // if skill based SimpleCurves are not set, flat chance gets used
        public float flatRangedBlockChance = 0.5f;

        // fatigue
        public bool useFatigueSystem = false; // if true, fatigue system gets used and the pawn cannot block if the damage would increase his current fatigue over the max fatigue
        public float maxFatigue = 100f;
        public float ifBlockedDamageToFatigueFactor = 1f; // 0 = no damge, 1 = full damage to fatigue
        public int fatigueResetAfterTicks = 2500; // default 1 ingame hour

        // damage absorb
        public float ifBlockedDamageToPawnFactor = 0f; // increase if the pawn should take damage when blocking, 0 = no damge, 1 = full damage to pawn
        public float ifBlockedDamageToShielFactor = 0f; // increase if the shield should take damage when blocking, 0 = no damge, 1 = full damage to shield


        // TODO block chance testing and block sound
    }
}
