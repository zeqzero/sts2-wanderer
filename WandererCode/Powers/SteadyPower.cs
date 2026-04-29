using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Wanderer.WandererCode.Powers;

/// <art>blur</art>
public class SteadyPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // Tick after the round fully resolves so that "next 2 turns" covers
    // the turn of application and the following turn.
    public override async Task AfterTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Enemy && Amount > 0)
        {
            await PowerCmd.Decrement(this);
        }
    }
}
