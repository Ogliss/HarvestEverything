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
        
    }
}
