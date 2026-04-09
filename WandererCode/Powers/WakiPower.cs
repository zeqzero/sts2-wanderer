using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Interfaces;

namespace Wanderer.WandererCode.Powers;

public class WakiPower : WandererPower, IStancePower
{
    public Stance Stance => Stance.Waki;
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ArgumentNullException.ThrowIfNull(Owner.Player);
        await WandererCmd.PickAndShiftCardsFromHand(new BlockingPlayerChoiceContext(), 1, Owner.Player, this, addKeywords: [ CardKeyword.Retain ]);

        foreach (var card in PileType.Hand.GetPile(Owner.Player).Cards.Where(c => c.Keywords.Contains(CardKeyword.Retain)))
        {
            card.EnergyCost.AddUntilPlayed(-1);
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            await WandererCmd.PickAndShiftCardsFromHand(new BlockingPlayerChoiceContext(), Amount, Owner.Player, this, addKeywords: [ CardKeyword.Retain ]);

            foreach (var card in PileType.Hand.GetPile(Owner.Player).Cards.Where(c => c.Keywords.Contains(CardKeyword.Retain)))
            {
                card.EnergyCost.AddUntilPlayed(-1);
            }
        }
    }
}