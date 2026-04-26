using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Patches;

[HarmonyPatch(typeof(ColorfulPhilosophers))]
internal static class ColorfulPhilosophers_Patches
{
    [HarmonyPatch("CardPoolColorOrder", MethodType.Getter)]
    [HarmonyPostfix]
    private static void Postfix(ref IEnumerable<CardPoolModel> __result)
    {
        __result = __result.Append(ModelDb.CardPool<WandererCardPool>());
    }
}
