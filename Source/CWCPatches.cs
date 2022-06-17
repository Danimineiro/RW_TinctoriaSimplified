using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace TinctoriaSimplified
{
    public static class CWCPatches
    {
        public static bool CWCPresent(HashSet<Assembly> assemblys) => assemblys.Any((assembly) => assembly.GetName().Name == "CraftWithColor");

        public static bool AmmendIngredientsPatch(ref List<IngredientCount> __result, List<IngredientCount> original)
        {
            __result = new List<IngredientCount>(original);
            return false;
        }

        public static void RunCWCPatches(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method("CraftWithColor.BillAddition:AmendIngredients"), new HarmonyMethod(typeof(CWCPatches), nameof(AmmendIngredientsPatch)));
        }
    }
}