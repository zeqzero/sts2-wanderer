using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Interfaces;

namespace Wanderer.WandererCode.Powers;

/// <art>sword at 7 o'clock</art>
public class WakiPower : WandererPower, IStancePower
{
    private static readonly LocString ShiftAndRetainPrompt = new("card_selection", "WANDERER-TO_SHIFT_AND_RETAIN");

    public Stance Stance => Stance.Waki;
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            ArgumentNullException.ThrowIfNull(Owner.Player);
            await RunAsHookAction(ctx => ShiftAndRetain(ctx, (int)amount));
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            await ShiftAndRetain(choiceContext, Amount);
        }
    }

    private async Task ShiftAndRetain(PlayerChoiceContext choiceContext, int count)
    {
        await WandererCmd.PickAndShiftCardsFromHand(choiceContext, count, Owner.Player!, this, addKeywords: [ CardKeyword.Retain ], prompt: ShiftAndRetainPrompt);

        foreach (var card in PileType.Hand.GetPile(Owner.Player!).Cards.Where(c => c.Keywords.Contains(CardKeyword.Retain)))
        {
            card.EnergyCost.AddUntilPlayed(-1);
        }
    }
}