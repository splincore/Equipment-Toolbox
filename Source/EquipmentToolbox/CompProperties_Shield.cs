﻿using RimWorld;
using System;
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

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if (meleeBlockSkillToUse == null) meleeBlockSkillToUse = SkillDefOf.Melee;
            if (rangedBlockSkillToUse == null) rangedBlockSkillToUse = SkillDefOf.Shooting;
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string configError in base.ConfigErrors(parentDef))
            {
                yield return configError;
            }
            if (meleeBlockSounds.Any(s => s.sustain)) yield return "The list of meleeBlockSounds contains a sustainer. Sustainers cannot be played when blocking.";
            if (rangedBlockSounds.Any(s => s.sustain)) yield return "The list of rangedBlockSounds contains a sustainer. Sustainers cannot be played when blocking.";
            if (drawWhenDrafted && graphicData == null) yield return "Shield should be drawn when drafted, but graphicData is null.";
            if (drawWhenUndrafted && graphicData == null && graphicDataUndrafted == null) yield return "Shield should be drawn when drafted, but graphicData and graphicDataUndrafted are null.";
            if (graphicData != null && graphicData.graphicClass != typeof(Graphic_Multi)) yield return "graphicData must have graphicClass 'Graphic_Multi'";
            if (graphicDataUndrafted != null && graphicDataUndrafted.graphicClass != typeof(Graphic_Multi)) yield return "graphicData must have graphicClass 'Graphic_Multi'";
            if (postBlockClass != null && !typeof(SpecialEffectsUtility).IsAssignableFrom(postBlockClass)) yield return "postBlockClass is not a subclass of 'EquipmentToolbox.SpecialEffectsUtility'";
        }

        // this comp can be used on primary and non primary equipment and on apparel, default config makes melee blockable with 50% flat chance

        // graphics
        public GraphicData graphicData; // also uses draw size and offsets, needs <graphicClass>Graphic_Multi</graphicClass>
        public bool drawWhenDrafted = true;
        public bool drawWhenUndrafted = false;
        public GraphicData graphicDataUndrafted; // set if the undrafted graphic should be different, if null, the normal graphicData gets used when the shield should get rendered undrafted

        // block sounds: plays random sound in correspondending list
        public List<SoundDef> meleeBlockSounds = new List<SoundDef>(); // this is a list, so you can have different sounds for more immersion (a random sound from the list will be played)
        public List<SoundDef> rangedBlockSounds = new List<SoundDef>(); // leave lists empty if you don't want any sounds

        // blocking general
        // calculated block chance will be displayed on the item stats
        public bool canBlockMelee = true; // if the pawn can block melee damage
        public bool canBlockRanged = false; // if the pawn can block ranged damage
        public bool canBlockDrafted = true; // if the pawn can block while drafted
        public bool canBlockUndrafted = false; // if the pawn can block while undrafted
        public float blockAngleRange = 90f; // area in front of the direction in which the pawn is looking, which gets covered by the shield in degrees from 0 to 360, 360 = complete area around the pawn gets covered
        public float blockAngleOffsetDrafted = 0f; // rotates covered area around the pawn clockwise, from 0 to 359, 180 would mean the shield covers the pawns back
        public float blockAngleOffsetUndrafted = 0f; // if the pawn carries the shield on its back when undrafted you could rotate the block angle by 180 degrees
        public bool explosionsAreConsideredAsRanged = true; // set to false if explosions should be considered melee

        // block chances, 0 means 0% and 1 means 100%, block chance is the average between skill block chance and quality block chance
        // a high quality shield can compensate an unskilled pawn, a low quality shield can hinder a skilled pawn
        public SimpleCurve curveSkillBasedMeleeBlockChance; // has value <points> which is a list with pairs <li>(skill level, block chance)</li> skill level is from 0 to 20
        public SimpleCurve curveSkillBasedRangedBlockChance; // SimpleCurve needs only 1 entry to work but that would mean a flat block chance, more entries in the list means a more precise skill based block chance
        public SkillDef meleeBlockSkillToUse; // just write the name of the Skill here like "Melee" or Shooting, default is melee
        public SkillDef rangedBlockSkillToUse; // defualt is shooting
        public SimpleCurve curveQualityBasedMeleeBlockChance; // same as skill based curve but this time the list needs <li>(quality value, block chance)</li> lowest quality value is 1 for awful and highest is 7 for legendary
        public SimpleCurve curveQualityBasedRangedBlockChance; // if quality curves are not set, quality of the shield is not taken into account when blocking
        public float flatMeleeBlockChance = 0.5f; // if skill based SimpleCurves are not set, flat chance gets used
        public float flatRangedBlockChance = 0.5f; // same goes for ranged

        // fatigue
        public bool useFatigueSystem = false; // if true, fatigue system gets used and the pawn cannot block if the damage would increase his current fatigue over the max fatigue
        public float maxFatigue = 100f; // the fatigue rises with each block according to the damage of the attack times the fatigueFactor (the next stat)
        public float ifBlockedDamageToFatigueFactor = 1f; // 0 = no damge, 1 = full damage to fatigue
        public int fatigueResetAfterTicks = 2500; // ticks from last damage to fatigue reset, default 1 ingame hour

        // damage absorb
        public float ifBlockedDamageToPawnFactor = 0f; // increase if the pawn should take damage even when blocking, 0 = no damge, 1 = full damage to pawn
        public float ifBlockedDamageToShielFactor = 0f; // increase if the shield should take damage when blocking, 0 = no damge, 1 = full damage to shield

        // special
        public bool ignoresOtherShields = false; // set to true if the shield should be "stackable" with other shields in terms of rendering AND blocking
        public Type postBlockClass = null; // you can make your own class that inherits from EquipmentToolbox.SpecialEffectsUtility to do your own stuff after a block event, format yournamespace.yourclassname

        // with configuring the CompProperties you could even make the PUBG pan, that only blocks bullets from behind when undrafted
    }
}
