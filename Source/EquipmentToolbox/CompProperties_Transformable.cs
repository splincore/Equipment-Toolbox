using Verse;

namespace EquipmentToolbox
{
    public class CompProperties_Transformable : CompProperties
    {
        public CompProperties_Transformable()
        {
            compClass = typeof(Comp_Transformable);
        }

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

        // Gizmo
        public string TransformLabel = "";
        public string TransformDesc = "";
        public string TransformIcon;

        // Transform
        public SoundDef transformSound;
        public ThingDef transformInto;
        public ThingDef transformSecondaryProduct = null;
        public float transformTime = 0f;
        public ThingDef needsWeaponEquipped = null;
        public bool comsumesWeaponEquipped = false;
        public ThingDef needsApparelEquipped = null;
        public bool comsumesApparelEquipped = false;

        // Special
        public int uniqueCompID = 1;
    }
}
