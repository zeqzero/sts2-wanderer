using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Wanderer.WandererCode.Powers;

/// <art>head with curly backward pointing arrow overlayed... copy Block Next Turn and Tank</art>
public class WandererNextTurnTargetHeadPower : WandererNextTurnApplyPower<TargetHeadPower>
{
    protected override async Task ApplyEffect()
    {
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<TargetHeadPower>(enemy, Amount, Owner, null);
        }
    }
}
