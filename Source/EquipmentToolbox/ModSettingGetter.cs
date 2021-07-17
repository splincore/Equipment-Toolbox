using Verse;

namespace EquipmentToolbox
{
    [StaticConstructorOnStartup]
    public static class ModSettingGetter
    {
        static ModSettingGetter()
        {
            GetSettings();
        }

        public static void GetSettings()
        {
            showGizmosOnMultiselect = LoadedModManager.GetMod<EquipmentToolboxMod>().GetSettings<EquipmentToolboxModSettings>().showGizmosOnMultiselect;
            allowEquipmentReloading = LoadedModManager.GetMod<EquipmentToolboxMod>().GetSettings<EquipmentToolboxModSettings>().allowEquipmentReloading;
        }

        public static bool showGizmosOnMultiselect = true;
        public static bool allowEquipmentReloading = true;
    }
}
