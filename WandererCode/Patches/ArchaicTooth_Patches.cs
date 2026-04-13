using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using Wanderer.WandererCode.Cards;

namespace Wanderer.WandererCode.Patches;

[HarmonyPatch]
internal class ArchaicTooth_Patches
{
    // Extend the Transcendence mapping so ArchaicTooth can replace Kamae with Mu-gamae.
    [HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceStarterCard")]
    private static class GetStarterPatch
    {
        private static void Postfix(ref CardModel? __result, ArchaicTooth __instance,
            MegaCrit.Sts2.Core.Entities.Players.Player player)
        {
            // If the base method already found a match, don't override.
            if (__result != null) return;

            var kamaeId = ModelDb.Card<Kamae>().Id;
            __result = player.Deck.Cards.FirstOrDefault(c => c.Id == kamaeId);
        }
    }

    [HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceTransformedCard")]
    private static class GetTransformedPatch
    {
        private static bool Prefix(ref CardModel __result, CardModel starterCard)
        {
            var kamaeId = ModelDb.Card<Kamae>().Id;
            if (starterCard.Id != kamaeId) return true;

            CardModel muGamae = starterCard.Owner.RunState.CreateCard(ModelDb.Card<MuGamae>(), starterCard.Owner);
            if (starterCard.IsUpgraded)
            {
                MegaCrit.Sts2.Core.Commands.CardCmd.Upgrade(muGamae);
            }
            if (starterCard.Enchantment != null)
            {
                var enchantment = (MegaCrit.Sts2.Core.Models.EnchantmentModel)starterCard.Enchantment.MutableClone();
                MegaCrit.Sts2.Core.Commands.CardCmd.Enchant(enchantment, muGamae, enchantment.Amount);
            }
            __result = muGamae;
            return false;
        }
    }

    // Ensure Mu-gamae is excluded from DustyTome's random Ancient pool (it's a Transcendence card).
    [HarmonyPatch(typeof(ArchaicTooth), "TranscendenceCards", MethodType.Getter)]
    private static class TranscendenceCardsPatch
    {
        private static void Postfix(ref List<CardModel> __result)
        {
            __result.Add(ModelDb.Card<MuGamae>());
        }
    }
}
