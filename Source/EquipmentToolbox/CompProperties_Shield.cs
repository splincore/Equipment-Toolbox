using Verse;

namespace EquipmentToolbox
{
    public class CompProperties_Shield : CompProperties
    {
        public CompProperties_Shield()
        {
            this.compClass = typeof(CompShield);
        }

        // graphics
        public GraphicData graphicData;
        public bool drawWhenDrafted = true;
        public bool drawWhenUndrafted = false;

        // blocking
        public float blockAngleRange = 90f;
        public bool canBlockMelee = true;
        public bool canBlockRanged = false;

        // TODO shield take damage + %, block chance for melee/ranged, skill used for melee/ranged, flat % chance, quality factor
    }
}
