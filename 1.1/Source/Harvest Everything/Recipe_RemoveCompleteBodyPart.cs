using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Harvest_Everything
{
	// Token: 0x02000005 RID: 5
	public class Recipe_RemoveCompleteBodyPart : Recipe_Surgery
	{
		// Token: 0x06000009 RID: 9 RVA: 0x0000218B File Offset: 0x0000038B
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			yield break;
		}
	}
}
