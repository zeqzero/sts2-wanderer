using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Powers;

public class FudoshinPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
            return;

        var stancePower = WandererCmd.GetCurrentStancePower(Owner);
        if (stancePower is PowerModel power)
        {
            await PowerCmd.Apply(power, Owner, Amount, Owner, null);
        }
    }
}
