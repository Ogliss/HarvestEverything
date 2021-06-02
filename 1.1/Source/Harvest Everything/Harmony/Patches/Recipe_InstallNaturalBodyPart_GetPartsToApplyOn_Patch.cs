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
    public class Recipe_InstallNaturalBodyPart_GetPartsToApplyOn_Patch
    {
        public static IEnumerable<BodyPartRecord> Postfix(IEnumerable<BodyPartRecord> __result, Pawn pawn, RecipeDef recipe)
        {
            foreach (BodyPartRecord part in __result)
            {
                bool flag = !HarmonyPatches.IsChildrenDamaged(pawn, part);
                if (flag)
                {
                    continue;
                }
                yield return part;
            }

            IEnumerable<BodyPartRecord> notMissingParts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).Where(x => x.def.spawnThingOnRemoved != null && recipe.appliedOnFixedBodyParts.Contains(x.def));
            foreach (BodyPartRecord part in notMissingParts)
            {
                bool flag = !HarmonyPatches.IsChildrenDamaged(pawn, part);
                if (flag || __result.Contains(part))
                {
                    continue;
                }
                yield return part;
            }
            yield break;
        }
        
    }
}
