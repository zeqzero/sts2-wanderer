using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Powers;

/// <art></art>
public class FlowPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterStanceLeft(Creature creature, Stance stance)
    {
        if (creature != Owner)
            return;

        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
