using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>death</tags>
/// <art>wanderer face palming, tight zoom on head</art>
[Pool(typeof(WandererCardPool))]
public class CountRegrets : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ WandererKeywords.Enshrined ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [ new CardsVar(2) ];

    public CountRegrets() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(2m);
    }
}