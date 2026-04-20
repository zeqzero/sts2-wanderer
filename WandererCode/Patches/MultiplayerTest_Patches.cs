// The multiplayer test scene (dev console: `multiplayer test`) hardcodes its
// character list to the five base-game characters, so the Wanderer can't be
// picked for local MP testing. This patch appends Wanderer to the paginator's
// private _characters array before _Ready iterates it — the existing loop then
// populates _options with Wanderer's locstring alongside the base entries.
#if DEBUG
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Debug.Multiplayer;

namespace Wanderer.WandererCode.Patches;

[HarmonyPatch]
internal class MultiplayerTest_Patches
{
    [HarmonyPatch(typeof(NMultiplayerTestCharacterPaginator), "_Ready")]
    private static class AddWandererPatch
    {
        private static readonly FieldInfo CharactersField =
            AccessTools.Field(typeof(NMultiplayerTestCharacterPaginator), "_characters");

        private static void Prefix(NMultiplayerTestCharacterPaginator __instance)
        {
            var existing = (CharacterModel[])CharactersField.GetValue(__instance)!;
            if (existing.Any(c => c is Character.Wanderer)) return;
            var expanded = existing.Append(ModelDb.Character<Character.Wanderer>()).ToArray();
            CharactersField.SetValue(__instance, expanded);
        }
    }
}
#endif
