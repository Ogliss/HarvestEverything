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
    public class Recipe_RemoveImplant_GetPartsToApplyOn_Patch
    {
        public static IEnumerable<BodyPartRecord> Postfix(IEnumerable<BodyPartRecord> __result, Pawn pawn, RecipeDef recipe)
        {
            foreach (BodyPartRecord part in __result)
            {
                yield return part;
            }
            yield break;
        }
        
    }
}
