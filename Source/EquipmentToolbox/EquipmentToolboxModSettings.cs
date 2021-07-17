using Verse;

namespace EquipmentToolbox
{
    public class EquipmentToolboxModSettings : ModSettings
    {
        public bool showGizmosOnMultiselect = true;
        public bool allowEquipmentReloading = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref showGizmosOnMultiselect, "showGizmosOnMultiselect", false, false);
            Scribe_Values.Look<bool>(ref allowEquipmentReloading, "allowEquipmentReloading", false, false);
        }
    }
}
