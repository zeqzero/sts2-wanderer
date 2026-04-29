using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>steady</tags>
/// <art>tobacco being pressed down in to a pipe</art>
/// <kanji>固</kanji>
[Pool(typeof(WandererCardPool))]
public class Tamp : WandererCard
{
    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromPower<SteadyPower>()];

    public Tamp() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var stancePower = WandererCmd.GetCurrentStancePower(Owner.Creature);
        if (stancePower is PowerModel)
        {
            await WandererCmd.EnterStance(Owner.Creature, stancePower.Stance, 1);
        }

        await PowerCmd.Apply<SteadyPower>(Owner.Creature, 2, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
