using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Keywords;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>shift</tags>
[Pool(typeof(WandererCardPool))]
public class Flailing : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<FlailingPower>(3)];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromPower<FlailingPower>(), WandererKeywords.ShiftHoverTip];

    public Flailing() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FlailingPower>(Owner.Creature, DynamicVars["FlailingPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FlailingPower"].UpgradeValueBy(2m);
    }
}
