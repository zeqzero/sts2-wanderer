using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Cards;

/// <tags>death, exhaust</tags>
[Pool(typeof(WandererCardPool))]
public class Seppuku : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public Seppuku() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await WandererCmd.RitualDeath(Owner.Creature);
    }
}
