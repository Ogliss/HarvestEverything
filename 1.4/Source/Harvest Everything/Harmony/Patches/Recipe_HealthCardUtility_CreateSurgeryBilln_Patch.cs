using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using JetBrains.Annotations;

namespace Harvest_Everything
{
    // HealthCardUtility.CreateSurgeryBill
    [HarmonyPatch(typeof(HealthCardUtility), "CreateSurgeryBill")]
    public class Recipe_HealthCardUtility_CreateSurgeryBilln_Patch
    {
        public static bool Prefix(Pawn medPawn, RecipeDef recipe, BodyPartRecord part, List<Thing> uniqueIngredients = null, bool sendMessages = true)
        {
            if (recipe.defName == "HarvestEverything")
            {
                List<Hediff> hediffs = new List<Hediff>();
                medPawn.health.hediffSet.GetHediffs(ref hediffs, x => x.def.spawnThingOnRemoved != null && x.GetType() == typeof(Hediff_Implant));
                foreach (var item in hediffs)
                {
                    //    Log.Message($"trying to create bill to remove {medPawn.Name}'s {item.LabelCap}");
                    RecipeDef def = DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(x => x.removesHediff == item.def);
                    if (def != null)
                    {
                        Bill_Medical bill_Medical = new Bill_Medical(def, null);
                        bill_Medical.part = item.part;
                        medPawn.BillStack.AddBill(bill_Medical);
                        //    Log.Message($"created bill to remove {medPawn.Name}'s {item.LabelCap}");
                    }
                }
                IEnumerable<BodyPartRecord> allParts = GetHarvestablePartsToApplyOn(medPawn).OrderByDescending(x=> OrderFor(x.def.tags));
                foreach (var item in allParts)
                {
                //    Log.Message($"trying to create bill to harvest {medPawn.Name}'s {item.Label}");
                    if (item.IsCorePart || allParts.Any(x => x == item.parent || x == item.parent.parent))
                    {
                        continue;
                    }
                    Bill_Medical bill_Medical = new Bill_Medical(RecipeDefOf.RemoveBodyPart, null);
                    bill_Medical.Part = item;
                    medPawn.BillStack.AddBill(bill_Medical);
                //    Log.Message($"created bill to harvest {medPawn.Name}'s {item.Label}");
                }
            //    Log.Message($"tried to create bills to harvest {medPawn.Name}'s Oregans. Bill Count: {medPawn.BillStack.Bills.Count}");
                return false;
            }
            return true;
        }

        public static int OrderFor(List<BodyPartTagDef> tags)
        {
            if (tags.Contains(BodyPartTagDefOf.BloodPumpingSource) || tags.Contains(BodyPartTagDefOf.ConsciousnessSource))
            {
                return 0;
            }
            if (tags.Contains(BodyPartTagDefOf.MetabolismSource))
            {
                return 1;
            }
            if (tags.Contains(BodyPartTagDefOf.BreathingSource))
            {
                return 2;
            }
            if (tags.Contains(BodyPartTagDefOf.BloodFiltrationKidney))
            {
                return 3;
            }
            return 4;
        }

        public static IEnumerable<BodyPartRecord> GetHarvestablePartsToApplyOn(Pawn pawn)
        {
            IEnumerable<BodyPartRecord> notMissingParts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null);
            using (IEnumerator<BodyPartRecord> enumerator = notMissingParts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    BodyPartRecord part = enumerator.Current;
                    if (HarmonyPatches.HasRemoveableDirectlyAddedPartFor(pawn, part))
                    {
                        yield return part;
                    }
                    else
                    if (part.def.HasModExtension<ModExtension>() && part.def.GetModExtension<ModExtension>().requireCleanChildrenToRemove)
                    {
                        if (HarmonyPatches.IsChildrenClean(pawn, part))
                        {
                            yield return part;
                        }
                    }
                    else
                    if (part.def.spawnThingOnRemoved != null || part.def.forceAlwaysRemovable)
                    {
                        yield return part;
                    }
                }
            }
            yield break;
        }
    }
}
