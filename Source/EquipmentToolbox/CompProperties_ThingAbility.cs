using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EquipmentToolbox
{
	public class CompProperties_ThingAbility : CompProperties
	{
		public CompProperties_ThingAbility()
		{
			this.compClass = typeof(CompThingAbility);
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
			if (verbProperties.verbClass != typeof(Verb_LaunchThingAbilityProjectile)) yield return "Thing abilities must be of verbClass 'EquipmentToolbox.Verb_LaunchThingAbilityProjectile'";
			if (uniqueCompID < 1) yield return "uniqueCompID must be 1 or greater.";
			if (parentDef.comps.Any(x => x is CompProperties_ThingAbility compProperties_ThingAbility && compProperties_ThingAbility != this && compProperties_ThingAbility.uniqueCompID == uniqueCompID)) yield return "uniqueCompID must be unique among the comps of this thing.";
			if (reloadTime <= 0) yield return "reloadTime must be greater than 0.";
			if (beginTargetingClass != null && !typeof(SpecialEffectsUtility).IsAssignableFrom(beginTargetingClass)) yield return "beginTargetingClass is not a subclass of 'EquipmentToolbox.SpecialEffectsUtility'";
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

		// AI props
		public bool canAiUse = true; // if the ai can use this ability (only in combat and non-player controlled)
		public bool canAiUseOnNonPawn = false; // if the ai can use this on other things than pawns, for example doors/walls
		public float commonalityOfAiUsage = 0.5f; // how much the ai will use the ability, 0 = basically never, 1 = always when possible

		// Special
		public bool cannotMiss = false; // if true, ability shots will always hit
		public int uniqueCompID = 1; // the ID for the comp (any positive number) must be unique, when you transform, the ammo from the ability comps with same IDs gets transferred

		// the actual verb
		public VerbProperties verbProperties; // like normal vanilla verbProperties, verbClass has to be EquipmentToolbox.Verb_LaunchThingAbilityProjectile (so the ammo consumption and cannotMiss work properly)
		public Type beginTargetingClass = null; // you can create your own class that inherits from EquipmentToolbox.PostAbilityUtility to do your own stuff when the pawn starts the targeting, format namespace.classname
	}
}
