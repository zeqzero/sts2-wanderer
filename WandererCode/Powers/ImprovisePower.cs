using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Wanderer.WandererCode.Powers;

public class ImprovisePower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterShifted(CardModel card)
    {
        if (card.Owner == Owner.Player)
        {
            await PlayerCmd.GainEnergy(Amount, Owner.Player);
        }
    }
}