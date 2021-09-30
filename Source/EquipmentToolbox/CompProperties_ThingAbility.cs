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
			if (beginTargetingSound != null && beginTargetingSound.sustain) yield return "beginTargetingSound is a sustainer. Sustainers cannot be played when targeting.";
			if (verbProperties.verbClass != typeof(Verb_LaunchThingAbilityProjectile)) yield return "Thing abilities must be of verbClass 'EquipmentToolbox.Verb_LaunchThingAbilityProjectile'";
			if (uniqueCompID < 1) yield return "uniqueCompID must be 1 or greater.";
			if (parentDef.comps.Any(x => x is CompProperties_ThingAbility compProperties_ThingAbility && compProperties_ThingAbility != this && compProperties_ThingAbility.uniqueCompID == uniqueCompID)) yield return "uniqueCompID must be unique among the comps of this thing.";
			if (reloadTime <= 0) yield return "reloadTime must be greater than 0.";
		}

		// this comp can be used on primary and non primary equipment and on apparel, default config makes melee blockable with 50% flat chance

		// Gizmo
		public string abilityLabel;
		public string abilityDesc;
		public string abilityIcon;
		public float abilityIconAngle = 0f;
		public Vector2 abilityIconOffset = new Vector2(0f, 0f);
		public Color? abilityColor;
		public KeyBindingDef hotKey;
		public bool displayGizmoWhileUndrafted = true;
		public bool displayGizmoWhileDrafted = true;		

		// Ammo
		public int maxCharges = 1;
		public int ammoCountToRefill = 0; // use only if a certain ammo count refills to full without considering remaining charges
		public int ammoCountPerCharge = 0; // use for how many ammo is needed for 1 charge
		public bool destroyOnEmpty = false;
		public ThingDef ammoDef;
		public bool canBeReloaded = true;
		public bool spawnWithFullAmmo = true;
		public string chargeNoun;
		public float reloadTime = 1; // seconds
		public SoundDef soundReload;

		// AI props
		public bool canAiUse = true;
		public bool canAiUseOnNonPawn = false;
		public float commonalityOfAiUsage = 0.5f;

		// BeginTargeting
		public SoundDef beginTargetingSound; // sound to be played when the pawn starts targeting
		public FleckDef beginTargetingFleck; // default fleck rotation is same as pawn
		public float beginTargetingFleckSize = 1;
		public float beginTargetingFleckRotationRate = 0;
		public float beginTargetingFleckVelocityAngle = 0;
		public float beginTargetingFleckVelocitySpeed = 0;
		public Vector3 fleckNorthOffset = new Vector3(0f, 0f, 0f);
		public Vector3 fleckEastOffset = new Vector3(0f, 0f, 0f);
		public Vector3 fleckSouthOffset = new Vector3(0f, 0f, 0f);
		public Vector3 fleckWestOffset = new Vector3(0f, 0f, 0f);

		// Special
		public bool cannotMiss = false; // if true, ability shots will always hit
		public int uniqueCompID = 1; // the ID for the comp (any positive number), so when you transform, the ammo from the comps with same IDs gets transferred

		// the actual verb
		public VerbProperties verbProperties;
	}
}
