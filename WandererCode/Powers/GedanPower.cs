using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Wanderer.WandererCode.Powers;

// Whenever you fully block an attack, apply 2 weak and 2 vulnerable
public class GedanPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<VulnerablePower>() ];

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner.Player.Creature && dealer.IsEnemy && result.WasFullyBlocked)
        {
            Flash();
            await PowerCmd.Apply<WeakPower>(dealer, 2, target, null);
            await PowerCmd.Apply<VulnerablePower>(dealer, 2, target, null);
        }
    }
}