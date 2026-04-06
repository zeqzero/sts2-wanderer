using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Cards;

namespace Wanderer.WandererCode.Powers;

public class GedanDexterityPower : WandererTemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<EnterGedan>();
}
