using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Wanderer.WandererCode.Powers;

public class WakiPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ArgumentNullException.ThrowIfNull(Owner.Player);
        var choiceContext = new BlockingPlayerChoiceContext();
        var cards = await CardSelectCmd.FromHand(choiceContext, Owner.Player, new CardSelectorPrefs(SelectionScreenPrompt, Amount), filter: c => !c.IsSlyThisTurn, source: this);
        if (cards != null)
        {
            foreach (var card in cards)
            {
                CardCmd.ApplyKeyword(card, CardKeyword.Sly);
            }
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            var cards = await CardSelectCmd.FromHand(choiceContext, Owner.Player, new CardSelectorPrefs(SelectionScreenPrompt, Amount), filter: c => !c.IsSlyThisTurn, source: this);
            if (cards != null)
            {
                foreach (var card in cards)
                {
                    CardCmd.ApplyKeyword(card, CardKeyword.Sly);
                }
            }
        }
    }
}