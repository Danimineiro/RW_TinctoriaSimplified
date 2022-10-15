using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace TinctoriaSimplified
{
    public static class SelfDyingPatches
    {
        private static Pawn tempPawn;

        public static bool SDPresent(HashSet<Assembly> assemblys) => assemblys.Any((assembly) => assembly.GetName().Name == "SelfDyeing");

        public static void RunSDPatches(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method("SelfDyeing.JobGiver_SelfDyeing:TryGiveJob"), new HarmonyMethod(typeof(SelfDyingPatches), nameof(SelfDyingPatches.JobGiver_SelfDyingPrefix)), transpiler: new HarmonyMethod(typeof(SelfDyingPatches), nameof(SelfDyingPatches.JobGiver_SelfDyingPatch)));
        }

        public static void JobGiver_SelfDyingPrefix(Pawn pawn)
        {
            tempPawn = pawn;
            Log.Message($"Here with {pawn.Name}");
        }

        public static void JobGiver_SelfDyingFinalizer()
        {
            tempPawn = null;
        }

        public static IEnumerable<CodeInstruction> JobGiver_SelfDyingPatch(IEnumerable<CodeInstruction> instructions)
        {

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            List<CodeInstruction> modified = new List<CodeInstruction>();
            bool found = false;

            for (int i = 0; i < codes.Count && !found; i++)
            {
                modified.Add(codes[i]);

                found = codes[i].opcode == OpCodes.Ldsfld && codes[i].operand.ToString().Contains("Dye");
                if (!found) continue;

                List<Label> labels = new List<Label>();
                for (int j = 0; j < 4; j++)
                {
                    labels.AddRange(modified.Pop().labels);
                }

                CodeInstruction instruction = CodeInstruction.Call(() => GetReplacementJob(tempPawn));
                instruction.labels = labels;

                modified.Add(instruction);

                labels.Clear();
                for (int j = i; j < codes.Count; j++)
                {
                    if (codes[j].labels is null) continue;
                    labels.AddRange(codes[j].labels);
                }

                modified.Add(new CodeInstruction(OpCodes.Ret)
                {
                    labels = labels
                });
            }

            for (int i = 0; i < modified.Count; i++)
            {
                yield return modified[i];
            }

            yield break;
        }

        private static Job GetReplacementJob(Pawn _)
        {
            Log.Message($"Here with {tempPawn.Name}");
            return null;
        }
    }
}
