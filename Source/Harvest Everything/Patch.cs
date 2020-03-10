using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Harvest_Everything
{
	// Token: 0x02000004 RID: 4
	[StaticConstructorOnStartup]
	class Patch
	{
        // Token: 0x06000003 RID: 3 RVA: 0x0000205C File Offset: 0x0000025C Recipe_RemoveHediff
        static Patch()
		{
			Harmony harmonyInstance = new Harmony("com.github.ianjazz246.harvest_everything");
            MethodInfo method = AccessTools.TypeByName("RimWorld.Recipe_RemoveBodyPart").GetMethod("GetPartsToApplyOn");
            MethodInfo method2 = typeof(Patch).GetMethod("GetPartsPostfix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            bool flag = method2 == null;
            if (flag)
            {
                Log.Error("Postfix is null", false);
            }
            bool flag2 = harmonyInstance.Patch(method, null, new HarmonyMethod(method2)) == null;
            if (flag2)
            {
                Log.Error("Harvest Everything Harmony patch failed.", false);
            }
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

		// Token: 0x06000004 RID: 4 RVA: 0x000020EE File Offset: 0x000002EE
		private static IEnumerable<BodyPartRecord> GetAllChildParts(BodyPartRecord part)
		{
			yield return part;
			foreach (BodyPartRecord child in part.parts)
			{
				foreach (BodyPartRecord subChild in Patch.GetAllChildParts(child))
				{
					yield return subChild;
	
				}
			}
			yield break;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002100 File Offset: 0x00000300
		private static bool IsChildrenClean(Pawn pawn, BodyPartRecord part)
		{
			IEnumerable<BodyPartRecord> allChildParts = Patch.GetAllChildParts(part);
			foreach (BodyPartRecord bodyPartRecord in allChildParts)
            {
                bool AddedPart = pawn.health.hediffSet.HasDirectlyAddedPartFor(part);
                bool isClean = MedicalRecipesUtility.IsClean(pawn, bodyPartRecord);
                bool corePart = part != pawn.RaceProps.body.corePart;
                bool CanAmp = part.def.canSuggestAmputation;
                bool injury = pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_Injury) && d.def.isBad && d.Visible && d.Part == part && d.def.spawnThingOnRemoved == null);
                bool flag = !AddedPart && !isClean && !(corePart && CanAmp && injury);

            //    Log.Message(string.Format("{0}'s {6} on {7}, AddedPart: {1}, isClean: {2}, corePart: {3}, CanAmp: {4}, injury: {5} flag == {8}", pawn.NameShortColored, AddedPart, isClean, corePart, CanAmp, injury, part.def.LabelShortCap, part.LabelShortCap, flag));
                if (flag)
                {
                    return false;
				}
			}
			return true;
		}

        // Token: 0x06000006 RID: 6 RVA: 0x00002168 File Offset: 0x00000368
        private static IEnumerable<BodyPartRecord> GetPartsPostfix(IEnumerable<BodyPartRecord> __result, Pawn pawn)
        {
            foreach (BodyPartRecord part in __result)
            {
                bool flag = part.def.HasModExtension<ModExtension>() && part.def.GetModExtension<ModExtension>().requireCleanChildrenToRemove;
                if (flag)
                {
                    bool flag2 = !Patch.IsChildrenClean(pawn, part) && !pawn.health.hediffSet.HasDirectlyAddedPartFor(part);
                    if (flag2)
                    {
                        continue;
                    }
                }
                yield return part;
            }
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
        
        // Token: 0x06000007 RID: 7 RVA: 0x0000217F File Offset: 0x0000037F
        private static void ApplyOnPawnPostfix(Pawn pawn, BodyPartRecord part, List<Thing> ingredients)
		{
		}
	}
}
