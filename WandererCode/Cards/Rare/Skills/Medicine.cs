using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>shift, refill, flurry</tags>
[Pool(typeof(WandererCardPool))]
public class Medicine : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [WandererKeywords.Refill];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        WandererKeywords.ShiftHoverTip
    ];

    public Medicine() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
    }

    // Shift self after it lands in a post-play pile (matches the StealPeach pattern).
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card == this && oldPileType == PileType.Play && card.Owner == Owner)
        {
            await WandererCmd.ShiftCard(this, Owner);
        }
    }

    // Fires on the restored Medicine instance after the second shift reverts it via Refill.
    // Apply the Strength gain again so the total ends up doubled.
    public override async Task AfterRefilled(CardModel card)
    {
        if (card == this)
        {
            DynamicVars["StrengthPower"].BaseValue *= 2;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(1m);
    }
}
