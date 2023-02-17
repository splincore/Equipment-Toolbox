using RimWorld;
using Verse;

namespace EquipmentToolbox
{
    public class Projectile_RideBullet : Bullet
    {
        public ThingDef_RideBullet Def
        {
            get
            {
                return def as ThingDef_RideBullet;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (launcher is Pawn launcherPawn && launcherPawn != null)
            {
                launcherPawn.Position = Position;
                launcherPawn.Notify_Teleported(true, false);
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (launcher is Pawn launcherPawn && launcherPawn != null)
            {
                launcherPawn.Position = Position;
                launcherPawn.Notify_Teleported(true, false);
            }
            Destroy(DestroyMode.Vanish);
        }
    }
}
