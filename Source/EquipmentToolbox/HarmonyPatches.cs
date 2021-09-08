using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EquipmentToolbox
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("rimworld.carnysenpai.equipmenttoolbox");
            harmony.Patch(AccessTools.Method(typeof(Pawn_EquipmentTracker), "GetGizmos"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("EquipmentGetGizmos_PostFix")), null); // adds certain equipment gizmos to pawns
            harmony.Patch(AccessTools.Method(typeof(Pawn_ApparelTracker), "GetGizmos"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("ApparelGetGizmos_PostFix")), null); // adds certaom apparel gizmos to pawns
            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), "PreApplyDamage"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("PreApplyDamage_PostFix")), null); // shield block
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnAt"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("RenderPawnAt_PostFix")), null); // shield rendering
        }

        [HarmonyPostfix]
        public static void EquipmentGetGizmos_PostFix(Pawn_EquipmentTracker __instance, ref IEnumerable<Gizmo> __result) // adds equipment abilities to pawns
        {
            if (PawnAttackGizmoUtility.CanShowEquipmentGizmos())
            {
                if (Find.Selector.NumSelected > 1 && !ModSettingGetter.showGizmosOnMultiselect) return;
                Pawn pawn = __instance.pawn;
                List<Gizmo> newOutput = new List<Gizmo>();
                newOutput.AddRange(__result);
                foreach (ThingWithComps thingWithComps in __instance.AllEquipmentListForReading)
                {
                    foreach (ThingComp thingComp in thingWithComps.AllComps.FindAll(c => c is CompThingAbility || c is CompTransformable))
                    {
                        newOutput.AddRange(thingComp.CompGetGizmosExtra());
                    }
                }
                __result = newOutput;
            }
        }

        [HarmonyPostfix]
        public static void ApparelGetGizmos_PostFix(Pawn_ApparelTracker __instance, ref IEnumerable<Gizmo> __result) // adds apparel abilities to pawns
        {
            if (PawnAttackGizmoUtility.CanShowEquipmentGizmos())
            {
                if (Find.Selector.NumSelected > 1 && !ModSettingGetter.showGizmosOnMultiselect) return;
                Pawn pawn = __instance.pawn;
                List<Gizmo> newOutput = new List<Gizmo>();
                newOutput.AddRange(__result);
                foreach (ThingWithComps thingWithComps in __instance.WornApparel)
                {
                    foreach (ThingComp thingComp in thingWithComps.AllComps.FindAll(c => c is CompThingAbility || c is CompTransformable))
                    {
                        newOutput.AddRange(thingComp.CompGetGizmosExtra());
                    }
                }
                __result = newOutput;
            }
        }

        [HarmonyPostfix]
        public static void PreApplyDamage_PostFix(Pawn ___pawn, ref DamageInfo dinfo, ref bool absorbed) // shield block
        {
            bool triedToBlock = false;
            if (absorbed || dinfo.Def == DamageDefOf.Extinguish)
            {
                return;
            }
            if (___pawn.equipment.Primary != null && ___pawn.equipment.Primary.TryGetComp<CompShield>() is CompShield compShield)
            {
                absorbed = compShield.TryBlockDamage(ref dinfo, ___pawn);
                if (compShield.Props.postBlockClass != null) compShield.Props.postBlockClass.DoPostBlockEvent(___pawn, absorbed, ___pawn.equipment.Primary);
                if (absorbed) return;
                triedToBlock = true;
            }
            foreach (ThingWithComps thingWithComps in ___pawn.equipment.AllEquipmentListForReading)
            {
                if (thingWithComps.TryGetComp<CompShield>() is CompShield comp)
                {
                    if (triedToBlock)
                    {
                        if (comp.IgnoresOtherShields)
                        {
                            absorbed = comp.TryBlockDamage(ref dinfo, ___pawn);
                            if (comp.Props.postBlockClass != null) comp.Props.postBlockClass.DoPostBlockEvent(___pawn, absorbed, thingWithComps);
                            triedToBlock = true;
                        }
                    }
                    else
                    {
                        absorbed = comp.TryBlockDamage(ref dinfo, ___pawn);
                        if (comp.Props.postBlockClass != null) comp.Props.postBlockClass.DoPostBlockEvent(___pawn, absorbed, thingWithComps);
                        triedToBlock = true;
                    }                    
                    if (absorbed) return;
                }
            }
            foreach (ThingWithComps thingWithComps in ___pawn.apparel.WornApparel)
            {
                if (thingWithComps.TryGetComp<CompShield>() is CompShield comp)
                {
                    if (triedToBlock)
                    {
                        if (comp.IgnoresOtherShields)
                        {
                            absorbed = comp.TryBlockDamage(ref dinfo, ___pawn);
                            if (comp.Props.postBlockClass != null) comp.Props.postBlockClass.DoPostBlockEvent(___pawn, absorbed, thingWithComps);
                            triedToBlock = true;
                        }
                    }
                    else
                    {
                        absorbed = comp.TryBlockDamage(ref dinfo, ___pawn);
                        if (comp.Props.postBlockClass != null) comp.Props.postBlockClass.DoPostBlockEvent(___pawn, absorbed, thingWithComps);
                        triedToBlock = true;
                    }
                    if (absorbed) return;
                }
            }
        }

        [HarmonyPostfix]
        public static void RenderPawnAt_PostFix(Pawn ___pawn, Vector3 drawLoc) // shield rendering
        {
            bool triedToRender = false;
            if (___pawn.equipment == null) return;
            if (___pawn.equipment.Primary != null && ___pawn.equipment.Primary.TryGetComp<CompShield>() is CompShield compShield)
            {
                compShield.DrawAt(drawLoc, ___pawn.Rotation, ___pawn.Drafted);
                triedToRender = true;
            }
            foreach (ThingWithComps thingWithComps in ___pawn.equipment.AllEquipmentListForReading)
            {
                if (thingWithComps.TryGetComp<CompShield>() is CompShield comp)
                {
                    if (triedToRender)
                    {
                        if (comp.IgnoresOtherShields)
                        {
                            comp.DrawAt(drawLoc, ___pawn.Rotation, ___pawn.Drafted);
                            triedToRender = true;
                        }
                    }
                    else
                    {
                        comp.DrawAt(drawLoc, ___pawn.Rotation, ___pawn.Drafted);
                    }
                }
            }
            foreach (ThingWithComps thingWithComps in ___pawn.apparel.WornApparel)
            {
                if (thingWithComps.TryGetComp<CompShield>() is CompShield comp)
                {
                    if (triedToRender)
                    {
                        if (comp.IgnoresOtherShields)
                        {
                            comp.DrawAt(drawLoc, ___pawn.Rotation, ___pawn.Drafted);
                            triedToRender = true;
                        }
                    }
                    else
                    {
                        comp.DrawAt(drawLoc, ___pawn.Rotation, ___pawn.Drafted);
                    }
                }
            }
        }
    }
}
