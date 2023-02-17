using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace EquipmentToolbox
{
    public class JobGiver_AIUseTransformAbility : JobGiver_AIFightEnemy
    {
        protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
        {
            dest = IntVec3.Invalid;
            return false;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            UpdateEnemyTarget(pawn);
            Thing enemyTarget = pawn.mindState.enemyTarget;
            List<CompTransformable> compTransformables = GetUseableTransFormations(pawn, enemyTarget);
            if (compTransformables.Count > 0)
            {
                return compTransformables.RandomElement().GetTransformJob();                
            }
            return null;
        }

        public List<CompTransformable> GetUseableTransFormations(Pawn pawn, Thing target)
        {
            List<CompTransformable> possibleTransformables = new List<CompTransformable>();
            if (pawn.equipment != null)
            {
                foreach (ThingWithComps thingWithComps in pawn.equipment.AllEquipmentListForReading)
                {
                    foreach (CompTransformable compTransformable in thingWithComps.AllComps.FindAll(c => c is CompTransformable tmp && tmp.CanBeUsedByAIConsideringTarget(target)))
                    {
                        if (pawn.Drafted && compTransformable.Props.shouldAiAlwaysUseWhenDrafted) return new List<CompTransformable>() { compTransformable };
                        if (!pawn.Drafted && compTransformable.Props.shouldAiAlwaysUseWhenUnDrafted) return new List<CompTransformable>() { compTransformable };
                        possibleTransformables.Add(compTransformable);
                    }
                }
            }
            if (pawn.apparel != null)
            {
                foreach (ThingWithComps thingWithComps in pawn.apparel.WornApparel)
                {
                    foreach (CompTransformable compTransformable in thingWithComps.AllComps.FindAll(c => c is CompTransformable tmp && tmp.CanBeUsedByAIConsideringTarget(target)))
                    {
                        if (pawn.Drafted && compTransformable.Props.shouldAiAlwaysUseWhenDrafted) return new List<CompTransformable>() { compTransformable };
                        if (!pawn.Drafted && compTransformable.Props.shouldAiAlwaysUseWhenUnDrafted) return new List<CompTransformable>() { compTransformable };
                        possibleTransformables.Add(compTransformable);
                    }
                }
            }
            return possibleTransformables;
        }
    }
}
