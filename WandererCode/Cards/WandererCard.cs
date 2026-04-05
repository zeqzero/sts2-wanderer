using BaseLib.Abstracts;
using BaseLib.Extensions;
using Wanderer.WandererCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Interfaces;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Wanderer.WandererCode.Cards;

public abstract class WandererCard(int cost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true, bool autoAdd = true) :
    CustomCardModel(cost, type, rarity, target, showInCardLibrary, autoAdd), IWandererEventListener
{
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

    public virtual async Task AfterEnteredShinigami(Creature creature)
    {
    }

    public virtual async Task BeforeRitualDeath(Creature creature)
    {
    }

    public virtual async Task AfterStanceEntered(Creature creature, Stance stance)
    {
    }

    public virtual async Task AfterShifted(CardModel card)
    {
    }
}
