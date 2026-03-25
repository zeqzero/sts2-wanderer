using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Wanderer.WandererCode.Powers;

/// <summary>
/// Attacks deal double damage. Gain vulnerable at the end of your turn.
/// </summary>
public class JodanPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != Owner)
        {
            return 1m;
        }

        if (!props.IsPoweredAttack_())
        {
            return 1m;
        }

        if (cardSource == null)
        {
            return 1m;
        }

        return 2m;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
{
    if (side == Owner.Side)
    {
        await PowerCmd.Apply<VulnerablePower>(Owner, 1, Owner, null);
    }
}
}