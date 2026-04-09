using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Wanderer.WandererCode.Powers;

public class NoPlanPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterShifted(CardModel card)
    {
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
