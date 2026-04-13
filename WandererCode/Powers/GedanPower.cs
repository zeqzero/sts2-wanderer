using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Interfaces;

namespace Wanderer.WandererCode.Powers;

/// <art></art>
public class GedanPower : WandererPower, IStancePower
{
    public Stance Stance => Stance.Gedan;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ HoverTipFactory.FromPower<CounterPower>() ];

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            await PowerCmd.Apply<CounterPower>(Owner, amount, Owner, null);
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await PowerCmd.Apply<CounterPower>(Owner, Amount, Owner, null);
    }
}
