using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace EquipmentToolbox
{
    public class CompShield : ThingComp
    {
        public CompProperties_Shield Props
        {
            get
            {
                return props as CompProperties_Shield;
            }
        }

        public bool CanDrawNow(bool isDrafted)
        {
            if (isDrafted && Props.drawWhenDrafted)
            {
                return true;
            }
            else if (!isDrafted && Props.drawWhenUndrafted)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DrawAt(Vector3 drawPos, Rot4 rot, bool isDrafted = false)
        {
            if (Props.graphicData == null || !CanDrawNow(isDrafted)) return;
            Material material = Props.graphicData.Graphic.MatAt(rot);
            Vector3 drawLoc = drawPos + Props.graphicData.DrawOffsetForRot(rot);
            Mesh mesh = Props.graphicData.Graphic.MeshAt(rot);
            Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(rot.AsInt, Vector3.up), material, 0);
        }

        public bool BlockDamage(DamageInfo damageInfo, Pawn pawn)
        {
            bool isRanged = damageInfo.Def.isExplosive || damageInfo.Def.isRanged || damageInfo.Instigator == null || !damageInfo.Instigator.Position.AdjacentTo8WayOrInside(pawn.Position);
            if (isRanged && !Props.canBlockRanged) return false;
            if (!isRanged && !Props.canBlockMelee) return false;

            if (Props.blockAngleRange <= 0f) return false;
            if (Props.blockAngleRange < 360f && damageInfo.Angle >= 0f)
            {
                float pawnAngle = pawn.Rotation.AsInt * 90f;
                float damageIncomingAngle = (damageInfo.Angle + 180) % 360;
                if (pawn.stances.curStance is Stance_Busy stance_Busy && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
                {
                    Vector3 a;
                    if (stance_Busy.focusTarg.HasThing)
                    {
                        a = stance_Busy.focusTarg.Thing.DrawPos;
                    }
                    else
                    {
                        a = stance_Busy.focusTarg.Cell.ToVector3Shifted();
                    }
                    if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                    {
                        pawnAngle = (a - pawn.DrawPos).AngleFlat();
                    }
                }
                float angleDiff = ((pawnAngle - damageIncomingAngle + 180f + 360f) % 360f) - 180f;
                if (angleDiff < -(Props.blockAngleRange / 2f) || angleDiff > (Props.blockAngleRange / 2f)) return false;
            }

            return false;
        }
    }
}
