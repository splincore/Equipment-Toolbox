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
		public int baseReloadTicks = 60;
		public SoundDef soundReload;

		// AI props
		public bool canAiUse = true;
		public bool canAiUseOnNonPawn = false;
		public float commonalityOfAiUsage = 0.5f;

		// BeginTargeting
		public SoundDef beginTargetingSound;
		public FleckDef beginTargetingFleck;
		public float beginTargetingFleckSize = 1;
		public Vector3 fleckNorthOffset = new Vector3(0f, 0f, 0f);
		public Vector3 fleckEastOffset = new Vector3(0f, 0f, 0f);
		public Vector3 fleckSouthOffset = new Vector3(0f, 0f, 0f);
		public Vector3 fleckWestOffset = new Vector3(0f, 0f, 0f);

		// Special
		public bool cannotMiss = false;
		public int uniqueCompID = 1;

		// the actual verb
		public VerbProperties verbProperties;
	}
}
