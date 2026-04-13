using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Wanderer.WandererCode.Powers;

/// <art>copy Demon Form</art>
public class RoninFormPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;


    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
            return;

        if (Owner.Player == null)
            return;

        CardPlayStartedEntry? cardPlayStartedEntry = CombatManager.Instance.History.CardPlaysStarted.LastOrDefault((CardPlayStartedEntry e) => e.CardPlay.Card.Owner.Creature == Owner && e.HappenedThisTurn(CombatState));
        if (cardPlayStartedEntry != null)
        {
            var card = cardPlayStartedEntry.CardPlay.Card;

            if (card.HasBeenRemovedFromState)
                card.HasBeenRemovedFromState = false;

            card.BaseReplayCount += Amount;
            card.ExhaustOnNextPlay = true;

            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }
}