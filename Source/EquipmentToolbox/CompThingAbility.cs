using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EquipmentToolbox
{
	public class CompThingAbility : ThingComp
	{
		public CompProperties_ThingAbility Props
		{
			get
			{
				return props as CompProperties_ThingAbility;
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

		public ThingDef AmmoDef
		{
			get
			{
				return Props.ammoDef;
			}
		}

		public Texture2D AbilityIcon
		{
			get
			{
				return ContentFinder<Texture2D>.Get(Props.abilityIcon, true);
			}
		}

		public bool HasAmmoRemaining
		{
			get
			{
				return remainingCharges > 0 || AmmoDef == null;
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

		public Verb_LaunchThingAbilityProjectile Verb
		{
			get
			{
				if (verb == null)
				{
					verb = (Verb_LaunchThingAbilityProjectile)Activator.CreateInstance(typeof(Verb_LaunchThingAbilityProjectile));
					verb.verbTracker = new VerbTracker(Wearer);
					verb.verbTracker.InitVerbsFromZero();
					verb.verbProps = Props.verbProperties;
					verb.caster = Wearer;
					verb.compThingAbility = this;
				}
				return verb;
			}
		}

		public string LabelRemaining
		{
			get
			{
				if (AmmoDef == null)
				{
					return "&#8734;";
				}
				return string.Format("{0} / {1}", RemainingCharges, MaxCharges);
			}
		}

		public override void PostPostMake()
		{
			base.PostPostMake();
			if (Props.spawnWithFullAmmo)
			{
				remainingCharges = MaxCharges;
			}
			else
			{
				remainingCharges = 0;
			}
		}

		public override string CompInspectStringExtra()
		{
			return "ChargesRemaining".Translate(Props.ChargeNounArgument) + ": " + LabelRemaining;
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
			yield return new StatDrawEntry(statCategoryDef, "Stat_Thing_ReloadChargesRemaining_Name".Translate(Props.ChargeNounArgument), LabelRemaining, "Stat_Thing_ReloadChargesRemaining_Desc".Translate(Props.ChargeNounArgument), 2749, null, null, false);
			yield break;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			bool drafted = Wearer.Drafted;
			if ((drafted && !Props.displayGizmoWhileDrafted) || (!drafted && !Props.displayGizmoWhileUndrafted))
			{
				yield break;
			}
			if (Verb.verbProps.hasStandardCommand)
			{
				yield return CreateVerbTargetCommand();
			}			
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "Debug: Reload to full",
					action = delegate ()
					{
						remainingCharges = MaxCharges;
					}
				};
			}
			yield break;
		}

		private Command_ThingAbility CreateVerbTargetCommand()
		{
            Command_ThingAbility command_ThingAbility = new Command_ThingAbility(this)
            {
                defaultLabel = Props.abilityLabel,
                defaultDesc = Props.abilityDesc,
                icon = AbilityIcon,
                iconAngle = Props.abilityIconAngle,
                iconOffset = Props.abilityIconOffset,
				verb = Verb
            };
            if (Props.abilityColor != null) command_ThingAbility.overrideColor = Props.abilityColor;
			if (Props.hotKey != null) command_ThingAbility.hotKey = Props.hotKey;
			if (!Wearer.IsColonistPlayerControlled)
			{
				command_ThingAbility.Disable(null);
			}
			else if (verb.verbProps.violent && Wearer.WorkTagIsDisabled(WorkTags.Violent))
			{
				command_ThingAbility.Disable("IsIncapableOfViolenceLower".Translate(Wearer.LabelShort, Wearer).CapitalizeFirst() + ".");
			}
			else if (!HasAmmoRemaining)
			{
				command_ThingAbility.Disable(DisabledReason(MinAmmoNeeded(), MaxAmmoNeeded()));
			}
			return command_ThingAbility;
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

		public void BeginTargeting()
		{
			if (Props.beginTargetingSound != null)
			{
				SoundInfo info = SoundInfo.InMap(new TargetInfo(Wearer.PositionHeld, Wearer.MapHeld, false), MaintenanceType.None);
				Props.beginTargetingSound.PlayOneShot(info);
			}
			if (Props.beginTargetingFleck != null)
			{
				Vector3 fleckOffset = new Vector3(0f, 0f, 0f);
				if (Wearer.Rotation == Rot4.North)
				{
					fleckOffset = Props.fleckNorthOffset;
				}
				else if (Wearer.Rotation == Rot4.East)
				{
					fleckOffset = Props.fleckEastOffset;
				}
				else if (Wearer.Rotation == Rot4.South)
				{
					fleckOffset = Props.fleckSouthOffset;
				}
				else if (Wearer.Rotation == Rot4.West)
				{
					fleckOffset = Props.fleckWestOffset;
				}
				FleckMaker.Static(Wearer.PositionHeld + fleckOffset.ToIntVec3(), Wearer.MapHeld, Props.beginTargetingFleck);
			}
		}
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref remainingCharges, "remainingCharges", -999, false);
            Scribe_Deep.Look<Verb_LaunchThingAbilityProjectile>(ref verb, "verb", new object[] { Props.verbProperties, Wearer, new VerbTracker(Wearer), this });
            Verb.compThingAbility = this;
            Verb.caster = Wearer;
            if (Scribe.mode == LoadSaveMode.PostLoadInit && remainingCharges == -999)
			{
				remainingCharges = MaxCharges;
			}
		}

		private int remainingCharges;
		private Verb_LaunchThingAbilityProjectile verb;
	}
}
