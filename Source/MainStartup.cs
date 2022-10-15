using HarmonyLib;
using Verse;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;
using System.Reflection.Emit;

namespace TinctoriaSimplified
{


    [StaticConstructorOnStartup]
    public static class MainStartup
    {
        private static readonly HashSet<Assembly> assemblys;
        private static readonly Harmony harmony;

        static MainStartup()
        {
            Harmony.DEBUG = true;
            assemblys = AppDomain.CurrentDomain.GetAssemblies().ToHashSet();
            harmony = new Harmony("dani.TinctoriaSimplified");

            RunVanillaPatches();
            if (CWCPatches.CWCPresent(assemblys)) CWCPatches.RunCWCPatches(harmony);
            if (SelfDyingPatches.SDPresent(assemblys)) SelfDyingPatches.RunSDPatches(harmony);

            Log.Message($"<color=orange>[TinctoriaSimplified]</color> Hello world! SD Detected: {SelfDyingPatches.SDPresent(assemblys)}");
        }

        public static void RunVanillaPatches()
        {
            harmony.Patch(typeof(JobGiver_OptimizeApparel).GetMethod(nameof(JobGiver_OptimizeApparel.TryCreateRecolorJob)), new HarmonyMethod(typeof(JobDriver_RecolorApparelNoDye), nameof(JobDriver_RecolorApparelNoDye.TryCreateRecolorJobPatch)));
            harmony.Patch(AccessTools.Method("RimWorld.JobGiver_DyeHair:TryGiveJob"), new HarmonyMethod(typeof(JobDriver_DyeHairNoDye), nameof(JobDriver_DyeHairNoDye.TryGiveJobPatch)));
            harmony.Patch(AccessTools.Method("RimWorld.Dialog_StylingStation:DrawApparelColor"), transpiler: new HarmonyMethod(typeof(WindowPatches), nameof(WindowPatches.DrawApparelColorPatch)));
            harmony.Patch(AccessTools.Method("RimWorld.Dialog_StylingStation:DrawHairColors"), transpiler: new HarmonyMethod(typeof(WindowPatches), nameof(WindowPatches.DrawHairColorsPatch)));
        }
    }
}
