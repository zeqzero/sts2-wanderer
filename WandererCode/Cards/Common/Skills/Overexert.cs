using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>exhaust</tags>
[Pool(typeof(WandererCardPool))]
public class Overexert : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public Overexert() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var pile = PileType.Discard.GetPile(Owner);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, pile.Cards, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();
        if (selected != null)
        {
            selected.ExhaustOnNextPlay = true;
            await CardCmd.AutoPlay(choiceContext, selected, null);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
