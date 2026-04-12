using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Powers;

public sealed class WandererNextTurnBlockPower : WandererPower, IWandererNextTurnPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => true;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == Owner.Side && AmountOnTurnStart != 0)
        {
            if (Amount >= 0)
            {
                await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
            }
            else
            {
                await CreatureCmd.LoseBlock(Owner, Amount);
            }

            if (!WandererCmd.ShouldPreserveNextTurnPowers(Owner))
            {
                if (Amount == AmountOnTurnStart)
                {
                    await PowerCmd.Remove(this);
                }
                else
                {
                    await PowerCmd.Apply(this, Owner, -AmountOnTurnStart, Owner, null);
                }
            }
        }
    }

    public async Task ApplyNow(PlayerChoiceContext choiceContext, Player player)
    {
        if (Amount >= 0)
        {
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
        }
        else
        {
            await CreatureCmd.LoseBlock(Owner, Amount);
        }

        if (!WandererCmd.ShouldPreserveNextTurnPowers(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}