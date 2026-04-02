using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Actions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace Wanderer.WandererCode.Powers;

public class RoninFormPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    private bool _appliedThisTurn = true;

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
            return;

        if (Owner.Player == null)
            return;

        // if (_appliedThisTurn)
        // {
        //     _appliedThisTurn = false;
        //     return;
        // }

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