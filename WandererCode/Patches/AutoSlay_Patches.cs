// AutoSlay support is gated behind `#if DEBUG` so it ships only in local
// (default) builds. To produce a release build without these patches, use
// `dotnet build -c Release`.
//
// To run AutoSlay locally:
//   1. Ensure the player has agreed to mod loading once in-game (otherwise
//      ModManager skips loading mods and these patches never apply).
//   2. Launch the game with `--autoslay` (optionally `--seed=<seed>` and
//      `--log-file=<path>`).
//   3. AutoSlay will drive a full run with the Wanderer and quit when done
//      (exit 0 on success, 1 on failure).
//
// The base game's autoslay branch in NGame.GameStartup() is gated by
// `!IsReleaseGame() && CommandLineHelper.HasArg("autoslay")`. IsReleaseGame()
// is hardcoded to true in shipped builds, so the IsReleaseGamePatch flips it
// to false when the autoslay flag is present.
//
// AutoSlayer.PlayMainMenuAsync random-picks one of the unlocked characters at
// the character-select screen. UnlockIfPossiblePatch biases that pick to the
// Wanderer by force-unlocking the Wanderer button and force-locking every
// other character button while a run is active.
#if DEBUG
using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.AutoSlay;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Wanderer.WandererCode.Patches;

[HarmonyPatch]
internal class AutoSlay_Patches
{
    [HarmonyPatch(typeof(NGame), nameof(NGame.IsReleaseGame))]
    private static class IsReleaseGamePatch
    {
        private static bool Prefix(ref bool __result)
        {
            if (!CommandLineHelper.HasArg("autoslay"))
                return true;

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(NCharacterSelectButton), nameof(NCharacterSelectButton.UnlockIfPossible))]
    private static class UnlockIfPossiblePatch
    {
        private static readonly FieldInfo IsLockedField =
            AccessTools.Field(typeof(NCharacterSelectButton), "_isLocked");

        private static void Postfix(NCharacterSelectButton __instance)
        {
            if (!AutoSlayer.IsActive)
                return;

            if (__instance.Character is Character.Wanderer)
                __instance.DebugUnlock();
            else
                IsLockedField.SetValue(__instance, true);
        }
    }

    // Wanderer mechanics (Waki stance, exhaust-on-play, etc.) can legitimately
    // exhaust the entire deck mid-combat. In real play you die a turn or two
    // later; in AutoSlay the player is immortal (PlatingPower/RegenPower 999
    // applied by CombatRoomHandler) so the combat would otherwise spin out
    // until the 100-turn cap + 30s "Combat did not end" timeout. Detect the
    // soft-lock at EndTurn time and abort fast.
    [HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.EndTurn))]
    private static class DeckExhaustionPatch
    {
        private static void Prefix(Player player)
        {
            if (!AutoSlayer.IsActive) return;
            if (CombatManager.Instance == null || !CombatManager.Instance.IsInProgress) return;

            PlayerCombatState state = player.PlayerCombatState;
            if (state == null) return;

            if (state.Hand.Cards.Count == 0
                && state.DrawPile.Cards.Count == 0
                && state.DiscardPile.Cards.Count == 0)
            {
                throw new InvalidOperationException(
                    $"AutoSlay: deck exhausted (hand+draw+discard=0, exhaust={state.ExhaustPile.Cards.Count}). Aborting run.");
            }
        }
    }
}
#endif
