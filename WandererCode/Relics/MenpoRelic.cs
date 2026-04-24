using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Relics;

[Pool(typeof(WandererRelicPool))]
public class MenpoRelic : WandererRelic
{
    private const int MaxRetainedBlock = 10;

    private int _enemyAttacksThisTurn;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    public override int DisplayAmount => _enemyAttacksThisTurn;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

    private void SetAttackCount(int value)
    {
        AssertMutable();
        _enemyAttacksThisTurn = value;
        Status = value == 0 ? RelicStatus.Active : RelicStatus.Disabled;
        InvokeDisplayAmountChanged();
    }

    public override Task BeforeCombatStart()
    {
        SetAttackCount(0);
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        if (side == CombatSide.Enemy)
        {
            SetAttackCount(0);
        }
        return Task.CompletedTask;
    }

    public override Task AfterAttack(AttackCommand command)
    {
        if (command.Attacker?.IsMonster == true && command.Results.Any(r => r.Receiver == Owner.Creature))
        {
            SetAttackCount(_enemyAttacksThisTurn + 1);
        }
        return Task.CompletedTask;
    }

    public override bool ShouldClearBlock(Creature creature)
    {
        if (creature != Owner.Creature)
            return true;
        return _enemyAttacksThisTurn > 0;
    }

    public override async Task AfterPreventingBlockClear(AbstractModel preventer, Creature creature)
    {
        if (this != preventer || creature != Owner.Creature)
            return;

        int block = creature.Block;
        if (block == 0)
            return;

        if (block > MaxRetainedBlock)
        {
            await CreatureCmd.LoseBlock(creature, block - MaxRetainedBlock);
        }
        Flash();
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        SetAttackCount(0);
        return Task.CompletedTask;
    }
}
