using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Cards;

namespace Wanderer.WandererCode.Powers;

/// <art>copy dexterity</art>
public class GedanDexterityPower : WandererTemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<EnterGedan>();
}
