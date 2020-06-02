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
            MethodInfo method2 = typeof(HarmonyPatches).GetMethod("RemoveBodyPartGetPartsPostfix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
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
            MethodInfo method4 = typeof(HarmonyPatches).GetMethod("RemoveImplantGetPartsPostfix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            bool flag3 = method3 == null;
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
                MethodInfo method6 = typeof(HarmonyPatches).GetMethod("InstallNaturalBodyPartGetPartsPostfix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                bool flag5 = method5 == null;
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

		// Token: 0x06000004 RID: 4 RVA: 0x000020EE File Offset: 0x000002EE
		private static IEnumerable<BodyPartRecord> GetAllChildParts(BodyPartRecord part)
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

        private static bool IsChildrenClean(Pawn pawn, BodyPartRecord part)
        {
            IEnumerable<BodyPartRecord> allChildParts = HarmonyPatches.GetAllChildParts(part);
            foreach (BodyPartRecord bodyPartRecord in allChildParts)
            {
                bool flag = !MedicalRecipesUtility.IsClean(pawn, bodyPartRecord);
                if (flag)
                {
                    return false;
                }
            }
            return true;
        }

        private static IEnumerable<BodyPartRecord> RemoveBodyPartGetPartsPostfix(IEnumerable<BodyPartRecord> __result, Pawn pawn, RecipeDef recipe)
        {
            foreach (BodyPartRecord part in __result)
            {
                bool flag = part.def.HasModExtension<ModExtension>() && part.def.GetModExtension<ModExtension>().requireCleanChildrenToRemove;
                if (flag)
                {
                    bool flag2 = !HarmonyPatches.IsChildrenClean(pawn, part);
                    if (flag2)
                    {
                        continue;
                    }
                }
                yield return part;
            }
            yield break;
        }
        
        // Token: 0x06000006 RID: 6 RVA: 0x00002168 File Offset: 0x00000368
        private static IEnumerable<BodyPartRecord> RemoveImplantGetPartsPostfix(IEnumerable<BodyPartRecord> __result, Pawn pawn)
        {
            foreach (BodyPartRecord item in __result)
            {
                yield return item;
            }
            yield break;
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002168 File Offset: 0x00000368
        private static IEnumerable<BodyPartRecord> InstallNaturalBodyPartGetPartsPostfix(IEnumerable<BodyPartRecord> __result, Pawn pawn, RecipeDef recipe)
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
            /*
            List<BodyPartRecord> applicableParts = null;
            if (notMissingParts.Any())
            {
                foreach (BodyPartRecord part in notMissingParts)
                {
                    bool flag = !Patch.IsChildrenDamaged(pawn, part);
                    if (flag)
                    {
                        continue;
                    }
                    if (applicableParts.NullOrEmpty() || !applicableParts.Contains(part))
                    {
                        applicableParts.Add(part);
                        yield return part;
                    }
                }
            }
            */
            /*
            List<Hediff> allHediffs = pawn.health.hediffSet.hediffs.Where(x => x is Hediff_Implant).ToList();
            int num;
            for (int i = 0; i < allHediffs.Count; i = num + 1)
            {
                Log.Message(string.Format("{0}'s {1} on {2}", pawn.NameShortColored, allHediffs[i].LabelCap, allHediffs[i].Part));
                if (allHediffs[i].def.spawnThingOnRemoved != null && allHediffs[i].Visible && allHediffs[i] is Hediff_Implant)
                {
                    yield return allHediffs[i].Part;
                }
                num = i;
            }
            */
            yield break;
        }
        private static bool IsChildrenDamaged(Pawn pawn, BodyPartRecord part)
        {
            IEnumerable<BodyPartRecord> allChildParts = HarmonyPatches.GetAllChildParts(part);
            foreach (BodyPartRecord bodyPartRecord in allChildParts)
            {
                //    bool flag = !MedicalRecipesUtility.IsClean(pawn, bodyPartRecord);
                bool flag = pawn.health.hediffSet.PartIsMissing(bodyPartRecord);
                if (flag)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
