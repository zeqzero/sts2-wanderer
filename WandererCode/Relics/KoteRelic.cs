using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Relics;

/// <art>samurai arm guard</art>
[Pool(typeof(WandererRelicPool))]
public class KoteRelic : WandererRelic
{
    private const int Threshold = 3;
    private const int HealAmount = 1;

    private int _attacksPlayedThisTurn;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    public override int DisplayAmount => _attacksPlayedThisTurn % Threshold;

    private void SetAttackCount(int value)
    {
        AssertMutable();
        _attacksPlayedThisTurn = value;
        Status = (value % Threshold == Threshold - 1) ? RelicStatus.Active : RelicStatus.Normal;
        InvokeDisplayAmountChanged();
    }

    public override Task BeforeCombatStart()
    {
        SetAttackCount(0);
        return Task.CompletedTask;
    }

    public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        SetAttackCount(0);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner) return;
        if (!CombatManager.Instance.IsInProgress) return;
        if (cardPlay.Card.Type != CardType.Attack) return;

        SetAttackCount(_attacksPlayedThisTurn + 1);
        if (_attacksPlayedThisTurn % Threshold == 0)
        {
            Flash();
            await CreatureCmd.Heal(Owner.Creature, HealAmount);
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        SetAttackCount(0);
        return Task.CompletedTask;
    }
}
