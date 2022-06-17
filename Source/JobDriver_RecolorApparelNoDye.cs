using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace TinctoriaSimplified
{
    public class JobDriver_RecolorApparelNoDye : JobDriver
	{

		private static Thing ClosestStylingStation(Pawn pawn) => GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.StylingStation), PathEndMode.Touch, TraverseParms.For(pawn), validator: (Thing thing) => !thing.IsForbidden(pawn) && pawn.CanReserve(thing) && pawn.CanReserveSittableOrSpot(thing.InteractionCell));

		public static bool TryCreateRecolorJobPatch(ref bool __result, Pawn pawn, out Job job, bool dryRun = false)
		{
			if (!ModLister.CheckIdeology("Apparel recoloring") || !pawn.apparel.AnyApparelNeedsRecoloring || !(ClosestStylingStation(pawn) is Thing thing))
			{
				job = null;
				__result = false;

				return false;
			}

			try
			{
				List<LocalTargetInfo> tmpQueueApparel = new List<LocalTargetInfo>();

				foreach (Apparel apparel in pawn.apparel.WornApparel.Where((ap) => ap.DesiredColor != null))
				{
					tmpQueueApparel.Add(apparel);
				}

				if (tmpQueueApparel.Count > 0)
				{
					if (dryRun)
					{
						job = null;
					}
					else
					{
						job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("TS_RecolorApparelNoDye"));
						job.GetTargetQueue(TargetIndex.A).AddRange(tmpQueueApparel);
						job.SetTarget(TargetIndex.B, thing);
						job.count = tmpQueueApparel.Count;
					}

					__result = true;

					return false;
				}
            }
            catch (Exception e)
			{
				Log.Error($"[TinctoriaSimplified] Failed to create job from driver: JobDriver_RecolorApparelNoDye for Pawn: {pawn?.Name}\nException: {e}");
			}

			//Job creation failed
			job = null;
			__result = false;
			return false;
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!pawn.Reserve(job.GetTarget(StylingStationInd), job, 1, -1, null, errorOnFailed)) return false;

			if (job.GetTarget(StylingStationInd).Thing is Thing station && station.def.hasInteractionCell && !pawn.ReserveSittableOrSpot(station.InteractionCell, job, errorOnFailed)) return false;

			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (!ModLister.CheckIdeology("Apparel recoloring")) yield break;

			this.FailOnDespawnedOrNull(StylingStationInd);
			Thing thing = job.GetTarget(StylingStationInd).Thing;

			//foreach (Toil toil in JobDriver_DoBill.CollectIngredientsToils(DyeInd, StylingStationInd, ApparelInd, true, false)) yield return toil;

			yield return Toils_Goto.GotoThing(StylingStationInd, PathEndMode.InteractionCell);
			Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(ApparelInd);
			yield return extract;

			Toil recolorToil = Toils_General.Wait(WorkTimeTicks, TargetIndex.None);
			recolorToil.PlaySustainerOrSound(SoundDefOf.Interact_RecolorApparel);
			recolorToil.WithProgressBarToilDelay(StylingStationInd);
			yield return recolorToil;

			yield return Toils_General.Do(delegate
			{
				job.GetTarget(ApparelInd).Thing.TryGetComp<CompColorable>().Recolor();
			});

			yield return Toils_Jump.JumpIfHaveTargetInQueue(ApparelInd, extract);
			extract = null;
			yield break;
		}

		public const TargetIndex ApparelInd = TargetIndex.A;
		public const TargetIndex StylingStationInd = TargetIndex.B;
		public const int WorkTimeTicks = 1000;
	}
}
