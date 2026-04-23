using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;
using MegaCrit.Sts2.Core.Saves;

namespace Wanderer.WandererCode.Patches;

[HarmonyPatch]
internal class NGeneralStatsGrid_Patches
{
    [HarmonyPatch(typeof(NGeneralStatsGrid), "LoadStats")]
    private static class LoadStatsPatch
    {
        private static readonly MethodInfo CreateCharacterSectionMethod =
            AccessTools.Method(typeof(NGeneralStatsGrid), "CreateCharacterSection");

        private static void Postfix(NGeneralStatsGrid __instance)
        {
            var progress = SaveManager.Instance.Progress;
            var wandererId = ModelDb.Character<Character.Wanderer>().Id;
            CreateCharacterSectionMethod.Invoke(__instance, [progress, wandererId]);
        }
    }
}
