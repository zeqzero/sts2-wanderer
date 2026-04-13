using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Powers;

/// <art></art>
public sealed class WandererNextTurnEnergyPower : WandererPower, IWandererNextTurnPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    public override async Task AfterEnergyResetLate(Player player)
    {
        if (player == base.Owner.Player)
        {
            if (base.Amount >= 0)
            {
                await PlayerCmd.GainEnergy(base.Amount, player);
            }
            else
            {
                await PlayerCmd.LoseEnergy(-base.Amount, player);
                Flash();
            }

            if (!WandererCmd.ShouldPreserveNextTurnPowers(Owner))
            {
                await PowerCmd.Remove(this);
            }
        }
    }

    public async Task ApplyNow(PlayerChoiceContext choiceContext, Player player)
    {
        if (base.Amount >= 0)
        {
            await PlayerCmd.GainEnergy(base.Amount, player);
        }
        else
        {
            await PlayerCmd.LoseEnergy(-base.Amount, player);
            Flash();
        }

        if (!WandererCmd.ShouldPreserveNextTurnPowers(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}