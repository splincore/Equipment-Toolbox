using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EquipmentToolbox
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("rimworld.carnysenpai.equipmenttoolbox");
            harmony.Patch(AccessTools.Method(typeof(Pawn_EquipmentTracker), "GetGizmos"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("GetGizmos_PostFix")), null); // adds abilities to pawns
        }

        [HarmonyPostfix]
        public static void GetGizmos_PostFix(Pawn_EquipmentTracker __instance, ref IEnumerable<Gizmo> __result) // adds equipment abilities to pawns
        {
            if (PawnAttackGizmoUtility.CanShowEquipmentGizmos())
            {
                if (Find.Selector.NumSelected > 1 && !ModSettingGetter.showGizmosOnMultiselect) return;
                Pawn pawn = __instance.pawn;
                List<Gizmo> newOutput = new List<Gizmo>();
                newOutput.AddRange(__result);
                foreach (ThingWithComps thingWithComps in __instance.AllEquipmentListForReading)
                {
                    foreach (ThingComp thingComp in thingWithComps.AllComps.FindAll(c => c is CompThingAbility))
                    {
                        newOutput.AddRange(thingComp.CompGetGizmosExtra());
                    }
                }             
                __result = newOutput;
            }
        }
    }
}
