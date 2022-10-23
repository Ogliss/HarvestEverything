using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;

namespace Harvest_Everything
{
    [HarmonyPatch(typeof(MedicalRecipesUtility), "IsClean")]
    public static class MedicalRecipesUtility_IsClean_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, BodyPartRecord part, ref bool __result)
        {
            if (__result)
            {
                __result = !HarmonyPatches.IsChildrenDamaged(pawn, part);
            }
        }
    }
}