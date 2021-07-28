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
		public int ammoCountToRefill = 0;
		public int ammoCountPerCharge = 0;
		public bool destroyOnEmpty = false;
		public ThingDef ammoDef;
		public bool spawnWithFullAmmo = true;
		public string chargeNoun;
		public int baseReloadTicks = 60;
		public SoundDef soundReload;

		// BeginTargeting
		public SoundDef beginTargetingSound;
		public FleckDef beginTargetingFleck;
		public float beginTargetingFleckSize = 1;
		public Vector3 fleckNorthOffset = new Vector3(0f, 0f, 0f);
		public Vector3 fleckEastOffset = new Vector3(0f, 0f, 0f);
		public Vector3 fleckSouthOffset = new Vector3(0f, 0f, 0f);
		public Vector3 fleckWestOffset = new Vector3(0f, 0f, 0f);

		// Special and Transform
		public bool cannotMiss = false;
		public int uniqueCompID = 1;
		public VerbProperties verbProperties;
	}
}
