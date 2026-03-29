using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <summary>
/// Apply all Next Turn powers immediately.
/// </summary>
[Pool(typeof(WandererCardPool))]
public class Preempt : WandererCard
{
    public Preempt() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach(var power in Owner.Creature.Powers.ToList())
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