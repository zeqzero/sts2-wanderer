using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Wanderer.WandererCode.Commands;

public static class WandererCmd
{
    public static async Task TransformToRandomFromPool(CardModel card, Player player)
    {
        var options = player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint);
        var transformation = new CardTransformation(card, options);
        await CardCmd.Transform(transformation.Yield(), player.RunState.Rng.CombatCardGeneration);
    }
}
