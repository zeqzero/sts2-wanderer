using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Wanderer.WandererCode.Powers;

/// <art>blur plus next turn draw</art>
public class LongDrawsPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is SteadyPower && power.Owner == Owner && amount > 0)
        {
            await RunAsHookAction(ctx => CardPileCmd.Draw(ctx, Amount, Owner.Player!));
        }
    }
}
