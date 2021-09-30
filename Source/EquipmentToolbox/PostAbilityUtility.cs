using Verse;

namespace EquipmentToolbox
{
    public class PostAbilityUtility
    {
        public virtual void DoPostTransformPreDestroyEvent(Pawn pawn, ThingWithComps transformableSource, ThingWithComps transformedInto, ThingWithComps secondaryProduct = null, Def neededEquippedItem = null)
        {
            // TODO Test
        }
        public virtual void DoPostBlockEvent(Pawn pawn, bool successfullyBlocked, ThingWithComps shield)
        {
            // TODO Test
        }
    }
}
