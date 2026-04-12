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

/// <tags>energy, draw, shift, refill, dance</tags>
[Pool(typeof(WandererCardPool))]
public class Tame : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new CardsVar(2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [WandererKeywords.Refill];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Kamae>(),
        WandererKeywords.ShiftHoverTip
    ];

    public Tame() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Put Kamae in hand
        if (!PileType.Hand.GetPile(Owner).Cards.Any(c => c is Kamae))
        {
            CardModel? kamae = PileType.Draw.GetPile(Owner).Cards.FirstOrDefault(c => c is Kamae)
                ?? PileType.Discard.GetPile(Owner).Cards.FirstOrDefault(c => c is Kamae);

            if (kamae != null)
            {
                await CardPileCmd.Add(kamae, PileType.Hand);
            }
        }

        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
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

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
