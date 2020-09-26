using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace Harvest_Everything
{
    public class Recipe_RemoveBodyPart_GetPartsToApplyOn_Patch
    {
        public static IEnumerable<BodyPartRecord> Postfix(IEnumerable<BodyPartRecord> __result, Pawn pawn, RecipeDef recipe) 
        {
            foreach (BodyPartRecord part in __result)
            {
                if (HarmonyPatches.HasRemoveableDirectlyAddedPartFor(pawn, part))
                {
                    yield return part;
                }
                else
                if (HarmonyPatches.HasAmputateableFor(pawn, part))
                {
                //    yield return part.def != BodyPartDefOf.Arm ? part : part.parent;
                    yield return part;
                }
                else
                if (part.def.HasModExtension<ModExtension>() && part.def.GetModExtension<ModExtension>().requireCleanChildrenToRemove)
                {
                    if (HarmonyPatches.IsChildrenClean(pawn, part))
                    {
                        //    yield return part.def != BodyPartDefOf.Arm ? part : part.parent;
                        yield return part;
                    }
                }
                else 
                if (part.def.spawnThingOnRemoved != null)
                {
                    //    yield return part.def != BodyPartDefOf.Arm ? part : part.parent;
                    yield return part;
                }
            }
            yield break;
            /*
            bool removeable = part.def.spawnThingOnRemoved != null || pawn.health.hediffSet.hediffs.Any(x => x.Part == part && (x.def == HediffDefOf.WoundInfection && part.def.canSuggestAmputation));

            if (removeable && part.def.HasModExtension<ModExtension>() && part.def.GetModExtension<ModExtension>().requireCleanChildrenToRemove)
            {
                if (!HarmonyPatches.IsChildrenClean(pawn, part))
                {
                    Log.Message("SKIPPING "+pawn + "'s " + part.LabelCap + " !IsChildrenClean");
                    removeable = false;
                }
            }
            if (removeable)
            {
                Log.Message(pawn + "'s " + part.LabelCap + " returning");
                yield return part.def == BodyPartDefOf.Arm ? part.parent : part;
            }
            */
            /*
            //    IEnumerable<BodyPartRecord> notMissingParts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null);
                IEnumerable<BodyPartRecord> notMissingParts = __result;
                using (IEnumerator<BodyPartRecord> enumerator = notMissingParts.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        BodyPartRecord part = enumerator.Current;
                        if (pawn.health.hediffSet.HasDirectlyAddedPartFor(part))
                        {
                            yield return part;
                        }
                        else if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, part))
                        {
                            yield return part;
                        }
                        else if (part != pawn.RaceProps.body.corePart && part.def.canSuggestAmputation && pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_Injury) && d.def.isBad && d.Visible && d.Part == part))
                        {
                            yield return part;
                        }
                    }
            */

        }

    }
}
