using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Cards;

namespace Wanderer.WandererCode.Powers;

public class ChudanStrengthPower : WandererTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<EnterChudan>();
}
