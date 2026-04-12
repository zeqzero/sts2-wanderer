using MegaCrit.Sts2.Core.Entities.Powers;

namespace Wanderer.WandererCode.Powers;

public class RetaliatePower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}
