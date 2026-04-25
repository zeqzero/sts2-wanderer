using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Relics;

[Pool(typeof(WandererRelicPool))]
public class ShimenawaRelic : WandererRelic
{
    private const int Threshold = 3;
    private const int Damage = 7;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    public override int DisplayAmount =>
        CombatManager.Instance.IsInProgress ? CountUniqueNextTurnPowers() : 0;

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner.Creature) return;
        if (!NextTurnPowers.Is(power)) return;
        InvokeDisplayAmountChanged();
    }

    // AfterSideTurnStart runs *after* SetupPlayerTurn's AfterPlayerTurnStart at player
    // turn start, and that's where next-turn powers silently self-remove. Refresh here
    // (Late) so the count reflects the post-cleanup state.
    public override async Task AfterSideTurnStartLate(CombatSide side, CombatState combatState)
    {
        if (side != CombatSide.Player) return;
        InvokeDisplayAmountChanged();
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player) return;
        if (CountUniqueNextTurnPowers() < Threshold) return;

        var enemies = Owner.Creature.CombatState?.HittableEnemies;
        if (enemies == null || enemies.Count == 0) return;

        Flash();
        await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), enemies, Damage, ValueProp.Unpowered, Owner.Creature);
    }

    private int CountUniqueNextTurnPowers()
    {
        return Owner.Creature.Powers
            .Where(p => NextTurnPowers.Is(p) && p.Amount != 0)
            .Select(p => p.GetType())
            .Distinct()
            .Count();
    }
}
