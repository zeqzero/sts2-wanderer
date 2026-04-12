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

public class JodanPower : WandererPower, IStancePower
{
    public Stance Stance => Stance.Jodan;
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [ new PowerVar<VigorPower>(7) ];

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ArgumentNullException.ThrowIfNull(Owner.Player);
        var choiceContext = new BlockingPlayerChoiceContext();
        var cardsToExhaust = await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, Amount), context: choiceContext, player: Owner.Player, filter: null, source: this);
        if (cardsToExhaust != null)
        {
            foreach (var card in cardsToExhaust)
            {
                await CardCmd.Exhaust(choiceContext, card);
                await PowerCmd.Apply<VigorPower>(Owner, DynamicVars["VigorPower"].BaseValue, Owner, null);
            }
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        ArgumentNullException.ThrowIfNull(Owner.Player);
        var cardsToExhaust = await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, Amount), context: choiceContext, player: Owner.Player, filter: null, source: this);
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