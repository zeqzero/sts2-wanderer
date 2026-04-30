using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>death, exhaust</tags>
/// <art>wanderer kneeling in resolute meditation, eyes closed, sword laid across knees, faint glow of acceptance</art>
/// <kanji>覚悟</kanji>
[Pool(typeof(WandererCardPool))]
public class Kakugo : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [WandererKeywords.Enshrined];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(2)];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromKeyword(WandererKeywords.Enshrined)];

    public Kakugo() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainMaxHp(Owner.Creature, DynamicVars.MaxHp.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.MaxHp.UpgradeValueBy(1);
    }
}
