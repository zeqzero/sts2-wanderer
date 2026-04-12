using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>shift, refill, draw, energy</tags>
[Pool(typeof(WandererCardPool))]
public class Hiccup : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new EnergyVar(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [WandererKeywords.Refill];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        WandererKeywords.ShiftHoverTip
    ];

    public Hiccup() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    // Shift self after it lands in a post-play pile
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card == this && oldPileType == PileType.Play && card.Owner == Owner)
        {
            await WandererCmd.ShiftCard(this, Owner);
        }
    }

    // Fires on the restored Hiccup instance after the second shift reverts it via Refill.
    public override async Task AfterRefilled(CardModel card)
    {
        if (card == this)
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Energy"].UpgradeValueBy(1m);
    }
}
