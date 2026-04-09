using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Wanderer.WandererCode.Powers;

public class FlailingPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterShifted(CardModel card)
    {
        var target = Owner.Player.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        if (target != null)
        {
            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_attack_slash");
            await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), target, Amount, ValueProp.Unpowered, Owner);
        }
    }
}
