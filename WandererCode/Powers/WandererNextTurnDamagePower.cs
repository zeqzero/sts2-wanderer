using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Powers;

public class WandererNextTurnDamagePower : WandererPower, IWandererNextTurnPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == Owner.Side && AmountOnTurnStart != 0)
        {
            Flash();
            var target = Owner.Player.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
            if (target != null)
            {
                VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_attack_slash");
                await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), target, Amount, ValueProp.Unpowered, Owner);
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
        Flash();
        var target = Owner.Player.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        if (target != null)
        {
            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_attack_slash");
            await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Unpowered, Owner);
        }

        if (!WandererCmd.ShouldPreserveNextTurnPowers(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
