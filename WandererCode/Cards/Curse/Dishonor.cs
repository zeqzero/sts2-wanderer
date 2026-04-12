using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>exhaust</tags>
[Pool(typeof(CurseCardPool))]
public class Dishonor : WandererCard
{
    public override int MaxUpgradeLevel => 0;

    public override CardPoolModel Pool => ModelDb.CardPool<CurseCardPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ CardKeyword.Exhaust ];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [ WandererKeywords.RemoveDishonorHoverTip ];

    public Dishonor() : base(1, CardType.Curse, CardRarity.Curse, TargetType.None, false, false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }
}