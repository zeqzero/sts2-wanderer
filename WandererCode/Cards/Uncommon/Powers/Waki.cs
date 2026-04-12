using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>shift</tags>
[Pool(typeof(WandererCardPool))]
public class Waki : WandererCard
{
    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromPower<WakiPower>()];

    public Waki() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        WandererCmd.WakiEnabled = true;

        if (WandererCmd.GetCurrentStancePower(Owner.Creature) is WakiPower)
        {
            await PowerCmd.Apply<WakiPower>(Owner.Creature, 1, Owner.Creature, this);
        }
        else
        {
            await WandererCmd.EnterStance(Owner.Creature, Stance.Waki, 1);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}