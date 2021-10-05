using Verse;

namespace EquipmentToolbox
{
    public class SpecialEffectsUtility
    {
        public virtual void DoPostTransformPreDestroyEvent(Pawn pawn, ThingWithComps transformableSource, ThingWithComps transformedInto, ThingWithComps secondaryProduct = null, Def neededEquippedItem = null)
        {
            
        }
        public virtual void DoPostBlockEvent(Pawn pawn, bool successfullyBlocked, ThingWithComps shield)
        {
            
        }

        public virtual void DoBeginTargetingEvent(Pawn caster, LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false)
        {

        }
    }
}
