using BaseLib.Abstracts;
using BaseLib.Extensions;
using Wanderer.WandererCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Interfaces;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Wanderer.WandererCode.Cards;

public abstract class WandererCard(int cost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true, bool autoAdd = true) :
    CustomCardModel(cost, type, rarity, target, showInCardLibrary, autoAdd), IWandererEventListener
{
    private List<IHoverTip> _runtimeHoverTips = [];

    protected sealed override IEnumerable<IHoverTip> ExtraHoverTips =>
        _runtimeHoverTips.Count > 0
            ? WandererExtraHoverTips.Concat(_runtimeHoverTips)
            : WandererExtraHoverTips;

    protected virtual IEnumerable<IHoverTip> WandererExtraHoverTips => [];

    // MemberwiseClone would otherwise share this list with the canonical, so hover tips
    // added at runtime would leak onto every instance of the class.
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _runtimeHoverTips = [.. _runtimeHoverTips];
    }

    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190

    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    public void AddRuntimeHoverTip(IHoverTip hoverTip) => _runtimeHoverTips.Add(hoverTip);
    public void ClearRuntimeHoverTips() => _runtimeHoverTips.Clear();

    public virtual async Task AfterEnteredShinigami(Creature creature)
    {
    }

    public virtual async Task BeforeRitualDeath(Creature creature)
    {
    }

    public virtual async Task AfterStanceLeft(Creature creature, Stance oldStance)
    {
    }

    public virtual async Task AfterStanceEntered(Creature creature, Stance stance)
    {
    }

    public virtual async Task AfterShifted(CardModel card)
    {
    }

    public virtual async Task AfterRefilled(CardModel card)
    {
    }

    // Shift self after the card lands in its post-play result pile. Gated on the card
    // having actually left Play: AutoPlay (Mayhem, DistilledChaos, Cascade, etc.) re-Adds
    // the card Play→Play before OnPlay runs, which fires AfterCardChangedPiles(old=Play)
    // prematurely — shifting then would orphan the replacement in the Play pile.
    protected async Task ShiftSelfAfterPlay(CardModel card, PileType oldPileType)
    {
        if (card == this
            && oldPileType == PileType.Play
            && card.Owner == Owner
            && card.Pile != null
            && card.Pile.Type != PileType.Play)
        {
            await WandererCmd.ShiftCard(this, Owner);
        }
    }
}
