using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Cards;

/// <tags>dishonor, death, exhaust</tags>
[Pool(typeof(WandererCardPool))]
public class PlayDead : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [ HoverTipFactory.FromCard<Dishonor>() ];

    public PlayDead() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // put all curses from draw pile in to hand
        List<CardModel> curseCards = PileType.Draw.GetPile(Owner).Cards.Where((CardModel c) => c.Type == CardType.Curse).ToList();

        if (IsUpgraded)
        {
            // also add curses from discard
            curseCards.AddRange(PileType.Discard.GetPile(Owner).Cards.Where((CardModel c) => c.Type == CardType.Curse));
        }

        await CardPileCmd.Add(curseCards, PileType.Hand);
        await WandererCmd.AddDishonor(Owner, CombatState);
        await WandererCmd.RitualDeath(Owner.Creature);
    }

    protected override void OnUpgrade()
    {
    }
}