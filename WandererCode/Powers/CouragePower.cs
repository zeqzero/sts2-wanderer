using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Wanderer.WandererCode.Powers;

// Lives on each enemy with the Wanderer as Source. When the Source dies, the enemy
// holding the power takes damage to itself. This mirrors DeathPactPower because
// powers on the dying creature are torn down before AfterDeath would otherwise hit them.
/// <art>skull + sword</art>
public class CouragePower : WandererPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public Creature? Source { get; set; }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Source) return;

        await Trigger(choiceContext);
    }

    public override async Task BeforeRitualDeath(Creature creature)
    {
        if (creature != Source) return;

        await RunAsHookAction(Trigger);
    }

    // Last until the start of the next player turn so a fatal enemy attack still triggers it.
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Enemy)
        {
            await PowerCmd.Remove(this);
        }
    }

    private async Task Trigger(PlayerChoiceContext choiceContext)
    {
        Flash();
        // CreatureCmd.Damage returns a no-op DamageResult when the dealer is dead,
        // so we pass null instead of Source (the Wanderer, who just died).
        await CreatureCmd.Damage(choiceContext, Owner, Amount, ValueProp.Unpowered, null, null);
        await PowerCmd.Remove(this);
    }
}
