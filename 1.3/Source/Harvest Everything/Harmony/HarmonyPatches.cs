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
	// Token: 0x02000004 RID: 4
	[StaticConstructorOnStartup]
	class HarmonyPatches
    {
        public static bool enabled_QuestionableEthics;
        // Token: 0x06000003 RID: 3 RVA: 0x0000205C File Offset: 0x0000025C Recipe_RemoveHediff
        static HarmonyPatches()
        {
            
            enabled_QuestionableEthics = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "kongmd.qee");
            Harmony harmony = new Harmony("com.rimwold.ogliss.harvest_everything");
            MethodInfo method = AccessTools.TypeByName("RimWorld.Recipe_RemoveBodyPart").GetMethod("GetPartsToApplyOn");
            MethodInfo method2 = typeof(Recipe_RemoveBodyPart_GetPartsToApplyOn_Patch).GetMethod("Postfix");
            bool flag = method2 == null;
            if (flag)
            {
                Log.Error("RemoveBodyPart Postfix is null", false);
            }
            bool flag2 = harmony.Patch(method, null, new HarmonyMethod(method2)) == null;
            if (flag2)
            {
                Log.Error("Harvest Everything RemoveBodyPart patch failed.", false);
            }
            MethodInfo method3 = AccessTools.TypeByName("RimWorld.Recipe_RemoveImplant").GetMethod("GetPartsToApplyOn");
            MethodInfo method4 = typeof(Recipe_RemoveImplant_GetPartsToApplyOn_Patch).GetMethod("Postfix");
            bool flag3 = method4 == null;
            if (flag3)
            {
                Log.Error("RemoveImplant Postfix is null", false);
            }
            bool flag4 = harmony.Patch(method3, null, new HarmonyMethod(method4)) == null;
            if (flag4)
            {
                Log.Error("Harvest Everything RemoveImplant patch failed.", false);
            }
            if (!enabled_QuestionableEthics)
            {
                MethodInfo method5 = AccessTools.TypeByName("RimWorld.Recipe_InstallNaturalBodyPart").GetMethod("GetPartsToApplyOn");
                MethodInfo method6 = typeof(Recipe_InstallNaturalBodyPart_GetPartsToApplyOn_Patch).GetMethod("Postfix");
                bool flag5 = method6 == null;
                if (flag5)
                {
                    Log.Error("InstallNaturalBodyPart Postfix is null", false);
                }
                bool flag6 = harmony.Patch(method5, null, new HarmonyMethod(method6)) == null;
                if (flag6)
                {
                    Log.Error("Harvest Everything InstallNaturalBodyPart patch failed.", false);
                }
            }
            else
            {
                Log.Message("QEE deteched");
            }
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            if (Prefs.DevMode) Log.Message(string.Format("Harvest Everything: successfully completed {0} harmony patches.", harmony.GetPatchedMethods().Select(new Func<MethodBase, Patches>(Harmony.GetPatchInfo)).SelectMany((Patches p) => p.Prefixes.Concat(p.Postfixes).Concat(p.Transpilers)).Count((Patch p) => p.owner.Contains(harmony.Id))), false);
        }

        public static bool HasAmputateableFor(Pawn pawn, BodyPartRecord part)
        {
            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                if (pawn.health.hediffSet.hediffs[i].Part == part && pawn.health.hediffSet.hediffs[i].def == HediffDefOf.WoundInfection && part.def.canSuggestAmputation)
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<BodyPartRecord> GetAllChildParts(BodyPartRecord part)
		{
			yield return part;
			foreach (BodyPartRecord child in part.parts)
			{
				foreach (BodyPartRecord subChild in HarmonyPatches.GetAllChildParts(child))
				{
					yield return subChild;
				}
			}
			yield break;
		}

        public static bool IsChildrenClean(Pawn pawn, BodyPartRecord part)
        {
            IEnumerable<BodyPartRecord> allChildParts = HarmonyPatches.GetAllChildParts(part);
            foreach (BodyPartRecord bodyPartRecord in allChildParts)
            {
                if (!MedicalRecipesUtility.IsClean(pawn, bodyPartRecord))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsChildrenDamaged(Pawn pawn, BodyPartRecord part)
        {
            IEnumerable<BodyPartRecord> allChildParts = HarmonyPatches.GetAllChildParts(part);
        //    Log.Message("Checking "+pawn+"'s "+part+"'s "+allChildParts.Count()+" children");
            foreach (BodyPartRecord bodyPartRecord in allChildParts)
            {
            //    Log.Message("Checking "+ bodyPartRecord);
                if (pawn.health.hediffSet.PartIsMissing(bodyPartRecord) || !IsClean(pawn, bodyPartRecord))
                {
                //    Log.Message(bodyPartRecord +" is damaged!");
                    return true;
                }
            }
            return false;
        }
        /*
        // Original MedicalRecipesUtility.IsClean(Pawn pawn, BodyPartRecord part)
        public static bool IsClean(Pawn pawn, BodyPartRecord part)
        {
            return !pawn.Dead && !(from x in pawn.health.hediffSet.hediffs
                                   where x.Part == part
                                   select x).Any<Hediff>();
        }

        */
        public static bool IsClean(Pawn pawn, BodyPartRecord part)
        {
            return !pawn.Dead && !(from x in pawn.health.hediffSet.hediffs
                                   where x.Part == part && (x.def.chronic || x.IsPermanent() || x.def == HediffDefOf.WoundInfection) && x.def.isBad
                                   select x).Any<Hediff>();
        }
        public static bool HasRemoveableDirectlyAddedPartFor(Pawn pawn, BodyPartRecord part)
        {
            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                if (pawn.health.hediffSet.hediffs[i].Part == part && pawn.health.hediffSet.hediffs[i] is Hediff_AddedPart)
                {
                    return true && pawn.health.hediffSet.hediffs[i].Part.def.spawnThingOnRemoved != null;
                }
            }
            return false;
        }
        // Token: 0x060011A2 RID: 4514 RVA: 0x000638B5 File Offset: 0x00061AB5
        public static bool PartOrAnyAncestorHasRemoveableDirectlyAddedParts(Pawn pawn, BodyPartRecord part)
        {
            return HasRemoveableDirectlyAddedPartFor(pawn, part) || (part.parent != null && PartOrAnyAncestorHasRemoveableDirectlyAddedParts(pawn, part.parent));
        }

        // Token: 0x060011A3 RID: 4515 RVA: 0x000638DB File Offset: 0x00061ADB
        public static bool AncestorHasRemoveableDirectlyAddedParts(Pawn pawn, BodyPartRecord part)
        {
            return part.parent != null && PartOrAnyAncestorHasRemoveableDirectlyAddedParts(pawn, part.parent);
        }
    }
}
