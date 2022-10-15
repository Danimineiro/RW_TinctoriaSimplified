using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TinctoriaSimplified
{
	public class JobDriver_DyeHairNoDye : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			LocalTargetInfo target = job.GetTarget(StylingStationInd);
			if (!pawn.Reserve(target, job, errorOnFailed: errorOnFailed)) return false;
			if (target.Thing is Thing thing && thing.def.hasInteractionCell && !pawn.ReserveSittableOrSpot(thing.InteractionCell, job, errorOnFailed)) return false;

			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (!ModLister.CheckIdeology("Hair dyeing"))
			{
				yield break;
			}

			this.FailOnDespawnedOrNull(StylingStationInd);
			yield return Toils_Goto.GotoThing(StylingStationInd, PathEndMode.InteractionCell);
			yield return Toils_General.Wait(300, StylingStationInd).PlaySustainerOrSound(SoundDefOf.Interact_RecolorApparel, 1f).WithProgressBarToilDelay(StylingStationInd, false, -0.5f);
			yield return Toils_General.Do(delegate
			{
				pawn.style.FinalizeHairColor();
			});
			yield break;
		}

		public static bool TryGiveJobPatch(ref Job __result, Pawn pawn)
		{
			if (!ModsConfig.IdeologyActive)
			{
				__result = null;
				return false;
			}

			if (pawn.style.nextHairColor != null)
			{
				Color? nextHairColor = pawn.style.nextHairColor;
				Color hairColor = pawn.story.HairColor;
				if (nextHairColor == null || (nextHairColor != null && !(nextHairColor.GetValueOrDefault() == hairColor)))
				{
					Thing stylingStation = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.StylingStation), PathEndMode.InteractionCell, TraverseParms.For(pawn), validator: (Thing target) => !target.IsForbidden(pawn) && pawn.CanReserve(target));
					if (stylingStation == null)
					{
						__result = null;
						return false;
					}

					Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("TS_DyeHairNoDye"), stylingStation);
					job.count = 1;
					__result = job;
					return false;
				}
			}
			__result = null;
			return false;
		}

		public const TargetIndex StylingStationInd = TargetIndex.A;
		public const int WorkTimeTicks = 300;
	}
}
