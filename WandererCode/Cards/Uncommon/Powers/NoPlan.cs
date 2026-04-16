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
/// <art>wanderer accidentally deflecting an attack while pourding from gourd in to a small cup, oblivious</art>
/// <kanji>無策</kanji>
[Pool(typeof(WandererCardPool))]
public class NoPlan : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<NoPlanPower>(3)];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromPower<NoPlanPower>(), WandererKeywords.ShiftHoverTip];

    public NoPlan() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<NoPlanPower>(Owner.Creature, DynamicVars["NoPlanPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NoPlanPower"].UpgradeValueBy(2m);
    }
}
