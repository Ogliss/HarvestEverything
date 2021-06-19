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
                if (part.def.spawnThingOnRemoved != null)
                {
                    yield return part;
                }
            }
            yield break;
        }

    }
}
