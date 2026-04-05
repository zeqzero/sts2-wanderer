using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Wanderer.WandererCode.Cards;

/// <tags>shift, exhaust</tags>
[Pool(typeof(StatusCardPool))]
public class Ofuda : WandererCard
{
    public override CardPoolModel Pool => ModelDb.CardPool<StatusCardPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public Ofuda() : base(0, CardType.Status, CardRarity.Token, TargetType.Self)
    {
    }
}
