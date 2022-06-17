using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace TinctoriaSimplified
{
    public static class WindowPatches
    {
        public static IEnumerable<CodeInstruction> DrawApparelColorPatch(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            bool found = false;

            for (int i = 0; i < codes.Count && !found; i++)
            {
                yield return codes[i];
                found = codes[i].opcode == OpCodes.Endfinally;
                if (!found) continue;

                List<Label> labels = new List<Label>();

                for (int j = i; j < codes.Count; j++)
                {
                    if (codes[j].labels is null) continue;
                    labels.AddRange(codes[j].labels);
                }

                CodeInstruction instruction = CodeInstruction.Call(() => Widgets.EndScrollView());
                instruction.labels = labels;
                yield return instruction;
                yield return new CodeInstruction(OpCodes.Ret);
            }

            yield break;
        }

        public static IEnumerable<CodeInstruction> DrawHairColorsPatch(IEnumerable<CodeInstruction> instructions)
        {

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            bool found = false;

            for (int i = 0; i < codes.Count && !found; i++)
            {
                yield return codes[i];
                found = codes[i].opcode == OpCodes.Pop;
                if (!found) continue;

                List<Label> labels = new List<Label>();

                for (int j = i; j < codes.Count; j++)
                {
                    if (codes[j].labels is null) continue;
                    labels.AddRange(codes[j].labels);
                }

                yield return new CodeInstruction(OpCodes.Ret)
                {
                    labels = labels
                };
            }

            yield break;
        }
    }
}