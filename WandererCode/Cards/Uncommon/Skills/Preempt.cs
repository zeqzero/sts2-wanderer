using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>nextturn</tags>
/// <art></art>
[Pool(typeof(WandererCardPool))]
public class Preempt : WandererCard
{
    public Preempt() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Process KiaiPower first so it can count other next-turn powers before they remove themselves.
        var powers = Owner.Creature.Powers.ToList();
        foreach (var power in powers.Where(p => p is WandererNextTurnKiaiPower))
        {
            if (power is IWandererNextTurnPower nextTurnPower && power.Amount != 0)
            {
                await nextTurnPower.ApplyNow(choiceContext, Owner);
            }
        }
        foreach (var power in powers.Where(p => p is not WandererNextTurnKiaiPower))
        {
            if (power is IWandererNextTurnPower nextTurnPower && power.Amount != 0)
            {
                await nextTurnPower.ApplyNow(choiceContext, Owner);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}