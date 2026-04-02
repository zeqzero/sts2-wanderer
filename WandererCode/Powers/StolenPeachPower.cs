using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Wanderer.WandererCode.Cards;

namespace Wanderer.WandererCode.Powers;

public class StolenPeachPower : TemporaryStrengthPower, ICustomPower, ICustomModel
{
    public override AbstractModel OriginModel => ModelDb.Card<StolenPeach>();

    protected override bool IsPositive => false;
}