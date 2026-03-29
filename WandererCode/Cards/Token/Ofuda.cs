using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Wanderer.WandererCode.Cards;

/// <summary>
/// An ofuda card that hides the original card's identity.
/// When played, exhausts. The original card is restored in the exhaust pile.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class Ofuda : WandererCard
{
    public override CardPoolModel Pool => ModelDb.CardPool<TokenCardPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ CardKeyword.Exhaust ];

    public Ofuda() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
    {
    }
}
