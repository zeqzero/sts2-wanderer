using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>death</tags>
/// <art>wanderer pouring water over hand from a bamboo ladle, shinto purification, zoomed in on hand and ladle</art>
/// <kanji>結界</kanji>
[Pool(typeof(WandererCardPool))]
public class Kekkai : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(WandererKeywords.Enshrined),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public Kekkai() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var selected = await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1),
            c => !c.Keywords.Contains(WandererKeywords.Enshrined) && !c.Keywords.Contains(WandererKeywords.Refill), this);

        foreach (var card in selected)
        {
            card.AddKeyword(WandererKeywords.Enshrined);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
