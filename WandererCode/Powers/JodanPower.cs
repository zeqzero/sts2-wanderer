using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Interfaces;

namespace Wanderer.WandererCode.Powers;

/// <art>sword at 11 o'clock</art>
public class JodanPower : WandererPower, IStancePower
{
    public Stance Stance => Stance.Jodan;
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [ new PowerVar<VigorPower>(7) ];
    
    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            ArgumentNullException.ThrowIfNull(Owner.Player);
            await RunAsHookAction(ctx => ExhaustForVigor(ctx, (int)amount));
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
            return;

        await ExhaustForVigor(choiceContext, Amount);
    }

    private async Task ExhaustForVigor(PlayerChoiceContext choiceContext, int amount)
    {
        var cardsToExhaust = await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, amount), context: choiceContext, player: Owner.Player!, filter: null, source: this);
        if (cardsToExhaust != null)
        {
            foreach (var card in cardsToExhaust)
            {
                await CardCmd.Exhaust(choiceContext, card);
                await PowerCmd.Apply<VigorPower>(Owner, DynamicVars["VigorPower"].BaseValue, Owner, null);
            }
        }
    }
}