using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Wanderer.WandererCode.Powers;

/// <art>copy Parry</art>
public class RetaliatePower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // Tick down after CounterPower.AfterTurnEnd has consumed the AOE effect for this turn.
    public override async Task AfterTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Enemy && Amount > 0)
        {
            await PowerCmd.Decrement(this);
        }
    }
}
