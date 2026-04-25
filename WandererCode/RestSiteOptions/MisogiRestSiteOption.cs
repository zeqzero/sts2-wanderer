using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using Wanderer.WandererCode.Cards;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Extensions;

public class MisogiRestSiteOption : RestSiteOption
{
    public const string Id = "MISOGI";
    public static readonly string IconResPath = "ui/rest_site/option_misogi.png".ImagePath();

    public MisogiRestSiteOption(Player owner) : base(owner)
    {
    }

    public override string OptionId => Id;

    public override IEnumerable<string> AssetPaths => [IconResPath];

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            description.Add("DishonorCount", Owner.Deck.Cards.OfType<Dishonor>().Count());
            description.Add("ShinigamiCurrentHp", WandererCmd.GetShinigamiCurrentHp(Owner.Creature));
            description.Add("ShinigamiMaxHp", WandererCmd.GetShinigamiMaxHp(Owner.Creature));
            return description;
        }
    }

    public override async Task<bool> OnSelect()
    {
        // Remove every Dishonor from the deck
        var dishonorCards = Owner.Deck.Cards.OfType<Dishonor>().ToList();
        foreach (var dishonorCard in dishonorCards)
        {
            await CardPileCmd.RemoveFromDeck(dishonorCard, true);
        }

        // Fully restore the persisted shinigami HP pool
        WandererCmd.FullyHealShinigami(Owner.Creature);

        return true;
    }
}
