using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EquipmentToolbox
{
    public class CompProperties_Transformable : CompProperties
    {
        public CompProperties_Transformable()
        {
            compClass = typeof(CompTransformable);
        }

        public NamedArgument ChargeNounArgument
        {
            get
            {
                return this.chargeNoun.Named("CHARGENOUN");
            }
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string configError in base.ConfigErrors(parentDef))
            {
                yield return configError;
            }
            if (abilityIcon == null) yield return "abilityIcon must not be null.";
            if (ammoDef != null && ammoCountToRefill == 0 && ammoCountPerCharge == 0) yield return "Thing ability uses ammo, but does not need ammo for reloading. This will cause issues when pawns try to reload this ability.";
            if (soundReload != null && soundReload.sustain) yield return "soundReload is a sustainer. Sustainers cannot be played when reloading.";
            if (transformSound != null && transformSound.sustain) yield return "transformSound is a sustainer. Sustainers cannot be played when transforming.";
            if (uniqueCompID < 1) yield return "uniqueCompID must be 1 or greater.";
            if (parentDef.comps.Any(x => x is CompProperties_Transformable compProperties_Transformable && compProperties_Transformable != this && compProperties_Transformable.uniqueCompID == uniqueCompID)) yield return "uniqueCompID must be unique among the comps of this thing.";
            if (reloadTime <= 0) yield return "reloadTime must be greater than 0.";
            if (postTransformClass != null && !typeof(SpecialEffectsUtility).IsAssignableFrom(postTransformClass)) yield return "postTransformClass is not a subclass of 'EquipmentToolbox.SpecialEffectsUtility'";
        }

        // this comp can be used on primary and non primary equipment and on apparel

        // Gizmo
        public string abilityLabel; // the label on the ability gizmo
        public string abilityDesc; // the description of the ability gizmo
        public string abilityIcon; // the icon of the ability gizmo
        public float abilityIconAngle = 0f; // clockwise rotation of the icon
        public Vector2 abilityIconOffset = new Vector2(0f, 0f); // moves the icon left/right/up/down
        public Color? abilityColor; // color of the label default: none, format (r, g, b) with r, g or b being between 0 and 1
        public KeyBindingDef hotKey; // if you want to assign a hotkey
        public bool displayGizmoWhileUndrafted = true; // if not displayed the ai also cannot use it in the undrafted state
        public bool displayGizmoWhileDrafted = true; // if not displayed the ai also cannot use it in the drafted state

        // Ammo
        // ammo count will be displayed on the gizmo and ammo which ammo is needed will be displayed on the item stats (together with the abilityLabel and ammo count)
        public int maxCharges = 1; // magazine size
        public int ammoCountToRefill = 0; // use only if a certain ammo count refills to full without considering remaining charges
        public int ammoCountPerCharge = 0; // use for how many ammo is needed for 1 charge
        public bool destroyOnEmpty = false; // if the item should be destroyed when the magazine is empty
        public ThingDef ammoDef; // if no ammo def is set, the ability will have infinite ammo
        public bool canBeReloaded = true; // if you dont want pawns to be able to reload the ability
        public bool spawnWithFullAmmo = true; // if the magazine should already be full when the weapon is crafted/created
        public string chargeNoun; // name of the ammo, for example "bullet" or "grenade"
        public float reloadTime = 1; // in seconds
        public SoundDef soundReload; // if not set, no sound will be played

        // Transform
        public ThingDef transformInto = null; // in which thing the item transform (equipment and apparel are allowed for all transformations and secondary products), quality and art will be transfered (also some modded comps)
        public ThingDef transformSecondaryProduct = null; // if a secondary thing will be created with the transformation (for example if you combine a shield into the weapon and want to decombine it)
        public float transformTime = 0f; // in seconds
        public SoundDef transformSound = null; // if not set, no sound will be played
        public ThingDef needsItemEquipped = null; // if a secondary thing will be needed on the pawn (equipment or apparel)
        public bool comsumesItemEquipped = false; // if a secondary thing will be consumed with the transformation (for example if you want combine a shield into the weapon)

        // AI props
        public bool canAiUse = false; // if the ai can use the transformation (only non-player controlled pawns, except out of combat and shouldAiAlwaysUse is defined)
        public bool shouldAiAlwaysUseWhenDrafted = false; // if you want to create a default combat state
        public bool shouldAiAlwaysUseWhenUnDrafted = false; // if you want to create a default peaceful state
        public float shouldAiUseWhenTargetCloserThanCells = 0f; // if you want the ai to use this transformation if the target if the target is close (for example to transform into a blade)
        public float shouldAiUseWhenTargetFartherThanCells = 999f; // if you want the ai to use this transformation if the target if the target is far away (for example to transform into a sniper)
        public float commonalityOfAiUsage = 0.5f; // how much the ai will use the ability, 0 = basically never, 1 = always when possible
        public int aiTransformCooldownTicks = 2500; // cooldown for ai, so it cannot get stuck in a transformation cycle, for example when targets change their distance alot, default 1 ingame hour

        // Special
        public int uniqueCompID = 1; // the ID for the comp (any positive number), so when you transform, the ammo from the comps with same IDs gets transferred
        public Type postTransformClass = null; // you can make your own class that inherits from EquipmentToolbox.PostAbilityUtility to do your own stuff after a transformation, format namespace.classname
    }
}
