using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Cards;

/// <tags>shift, exhaust</tags>
[Pool(typeof(TokenCardPool))]
public class Ofuda : WandererCard
{
    public override CardPoolModel Pool => ModelDb.CardPool<TokenCardPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public Ofuda() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
    {
    }
}
