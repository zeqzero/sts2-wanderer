using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace Wanderer.WandererCode.Patches;

[HarmonyPatch]
internal class ProgressSaveManager_Patches
{
    // The base game's ObtainCharUnlockEpoch builds an epoch id like "WANDERER2_EPOCH"
    // from the character id and calls EpochModel.Get on it, which throws for modded
    // characters that don't define those epochs. Skip the method entirely for Wanderer.
    [HarmonyPatch(typeof(ProgressSaveManager))]
    [HarmonyPatch("ObtainCharUnlockEpoch")]
    [HarmonyPatch([typeof(Player), typeof(int)])]
    private static class ObtainEpochPatch
    {
        private static bool Prefix(Player localPlayer)
        {
            return localPlayer.Character is not Character.Wanderer;
        }
    }
}
