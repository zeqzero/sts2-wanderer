using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Wanderer.WandererCode.Powers;

/// <art>copy Parry</art>
public class CounterPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    private bool _wasAttackedThisTurn;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            _wasAttackedThisTurn = false;
        }
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner)
        {
            _wasAttackedThisTurn = true;
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Enemy)
        {
            if (_wasAttackedThisTurn && Owner.Block > 0)
            {
                Flash();

                RetaliatePower? retaliatePower = Owner.GetPower<RetaliatePower>();
                bool aoe = retaliatePower != null && retaliatePower.Amount > 0;

                foreach (var _ in Enumerable.Range(0, Amount))
                {
                    if (aoe)
                    {
                        await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, Owner.Block, ValueProp.Unpowered, Owner, null);
                    }
                    else
                    {
                        var target = Owner.Player.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
                        if (target != null)
                        {
                            await CreatureCmd.Damage(choiceContext, target, Owner.Block, ValueProp.Unpowered, Owner);
                        }
                    }
                }
            }

            await PowerCmd.Remove(this);
        }
    }
}