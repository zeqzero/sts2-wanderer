using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>exhaust, draw</tags>
/// <art>wanderer pulls an ofuda from a misty crack in the floor, a chill draft rising up from the underworld</art>
/// <kanji>黄泉</kanji>
[Pool(typeof(WandererCardPool))]
public class YomiDraft : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<YomiDraftPower>(1)];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public YomiDraft() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<YomiDraftPower>(Owner.Creature, DynamicVars["YomiDraftPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
