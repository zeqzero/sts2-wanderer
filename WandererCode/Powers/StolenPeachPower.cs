using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Cards;

namespace Wanderer.WandererCode.Powers;

public class StolenPeachPower : WandererTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<StealPeach>();

    protected override bool IsPositive => false;
}
