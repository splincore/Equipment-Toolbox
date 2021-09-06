using UnityEngine;
using Verse;

namespace EquipmentToolbox
{
    public class EquipmentToolboxMod : Mod
    {
        readonly EquipmentToolboxModSettings equipmentToolboxModSettings;
        public EquipmentToolboxMod(ModContentPack content) : base(content)
        {
            equipmentToolboxModSettings = GetSettings<EquipmentToolboxModSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("ShowGizmosOnMultiselectLabel".Translate(), ref equipmentToolboxModSettings.showGizmosOnMultiselect, "ShowGizmosOnMultiselectTooltip".Translate());
            listingStandard.CheckboxLabeled("AllowEquipmentReloadingLabel".Translate(), ref equipmentToolboxModSettings.allowEquipmentReloading, "AllowEquipmentReloadingTooltip".Translate());
            listingStandard.End();
            ModSettingGetter.GetSettings();
        }

        public override string SettingsCategory()
        {
            return "Equipment Toolbox";
        }
    }
}
