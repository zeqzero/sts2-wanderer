using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Cards;

namespace Wanderer.WandererCode.Powers;

/// <art></art>
public class ChudanStrengthPower : WandererTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<EnterChudan>();
}
