using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Cards;

/// <tags>commit</tags>
/// <art>wanderer in Hasso stance, pipe visible, smoke trailing upward</art>
/// <kanji>踏</kanji>
[Pool(typeof(WandererCardPool))]
public class Plant : WandererCard
{
    public Plant() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var stancePower = WandererCmd.GetCurrentStancePower(Owner.Creature);
        if (stancePower is PowerModel power)
        {
            await WandererCmd.EnterStance(Owner.Creature, stancePower.Stance, 1);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
