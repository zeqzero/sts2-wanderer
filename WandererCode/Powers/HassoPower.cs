using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Interfaces;

namespace Wanderer.WandererCode.Powers;

/// <art>sword at 12 o'clock</art>
public class HassoPower : WandererPower, IStancePower
{
    public Stance Stance => Stance.Hasso;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [ new CardsVar(1) ];

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            await PowerCmd.Apply<WandererNextTurnDrawPower>(Owner, DynamicVars.Cards.BaseValue * amount, Owner, null);
        }
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
            return;

        await PowerCmd.Apply<WandererNextTurnDrawPower>(Owner, DynamicVars.Cards.BaseValue * Amount, Owner, null);
    }
}