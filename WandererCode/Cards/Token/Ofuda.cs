using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Wanderer.WandererCode.Cards;

/// <tags>transform, exhaust</tags>
[Pool(typeof(TokenCardPool))]
public class Ofuda : WandererCard
{
    public override CardPoolModel Pool => ModelDb.CardPool<TokenCardPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ CardKeyword.Exhaust ];

    public Ofuda() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
    {
    }
}
