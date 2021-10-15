using Verse;

namespace EquipmentToolbox
{
    public class SpecialEffectsUtility
    {
        // gets called after the transformed things are created, but before the old things are deleted
        public virtual void DoPostTransformPreDestroyEvent(Pawn pawn, ThingWithComps transformableSource, ThingWithComps transformedInto, ThingWithComps secondaryProduct = null, Def neededEquippedItem = null)
        {
            
        }

        // gets called after each block attempt
        public virtual void DoPostBlockEvent(Pawn pawn, bool successfullyBlocked, ThingWithComps shield)
        {
            
        }

        // gets called when the pawn starts to aim the ability (directly after a you click a valid target)
        public virtual void DoBeginTargetingEvent(Pawn caster, LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false)
        {

        }
    }
}
