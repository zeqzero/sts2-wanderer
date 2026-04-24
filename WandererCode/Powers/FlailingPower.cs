using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Wanderer.WandererCode.Powers;

/// <art>cartoon fight cloud</art>
public class FlailingPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterShifted(CardModel card)
    {
        if (Owner.Player == null) return;

        foreach (var handCard in PileType.Hand.GetPile(Owner.Player).Cards.Where(c => c.Type == CardType.Attack))
        {
            handCard.EnergyCost.AddThisTurnOrUntilPlayed(-Amount);
        }
    }
}
