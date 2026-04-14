using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>shift, refill, grows</tags>
/// <art>wanderer slamming in to an enemy ass-first, feminine hand sign (peace, heart, or see drunken miss ho)</art>
[Pool(typeof(WandererCardPool))]
public class HipCheck : WandererCard
{
    private const int _baseDamage = 7;

    private int _currentDamage = _baseDamage;

    private int _increasedDamage;

    [SavedProperty]
    public int CurrentDamage
    {
        get => _currentDamage;
        set
        {
            AssertMutable();
            _currentDamage = value;
            DynamicVars.Damage.BaseValue = _currentDamage;
        }
    }

    [SavedProperty]
    public int IncreasedDamage
    {
        get => _increasedDamage;
        set
        {
            AssertMutable();
            _increasedDamage = value;
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [WandererKeywords.Refills];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(CurrentDamage, ValueProp.Move),
        new IntVar("Increase", 2m)
    ];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        WandererKeywords.ShiftHoverTip
    ];

    public HipCheck() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash", null, "slash_attack.mp3")
            .Execute(choiceContext);
    }

    // Shift self after it lands in a post-play pile
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card == this && oldPileType == PileType.Play && card.Owner == Owner)
        {
            await WandererCmd.ShiftCard(this, Owner);
        }
    }

    // Fires on the restored HipCheck instance after the second shift reverts it via Refill.
    public override async Task AfterRefilled(CardModel card)
    {
        if (card != this) return;

        int increase = DynamicVars["Increase"].IntValue;
        BuffDamage(increase);

        // DeckVersion is null on Refill clones (AfterCloned clears it), so update all deck copies.
        foreach (var deckCard in Owner.Deck.Cards.OfType<HipCheck>())
        {
            deckCard.BuffDamage(increase);
        }

        await CardCmd.Exhaust(new BlockingPlayerChoiceContext(), this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Increase"].UpgradeValueBy(1m);
    }

    protected override void AfterDowngraded()
    {
        UpdateDamage();
    }

    private void BuffDamage(int extraDamage)
    {
        IncreasedDamage += extraDamage;
        UpdateDamage();
    }

    private void UpdateDamage()
    {
        CurrentDamage = _baseDamage + IncreasedDamage;
    }
}
