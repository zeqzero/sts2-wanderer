using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Powers;

/// <art>copy Corruption</art>
public class FudoshinPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
            return;

        var stancePower = WandererCmd.GetCurrentStancePower(Owner);
        if (stancePower != null)
        {
            await WandererCmd.EnterStance(Owner, stancePower.Stance, Amount);
        }
    }
}
