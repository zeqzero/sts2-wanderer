using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using Wanderer.WandererCode.Cards;
using Wanderer.WandererCode.Extensions;

public class MisogiRestSiteOption : RestSiteOption
{
    public static readonly string IconResPath = "ui/rest_site/option_misogi.png".ImagePath();

    public MisogiRestSiteOption(Player owner) : base(owner)
    {
    }

    public override string OptionId => "MISOGI";

    public override IEnumerable<string> AssetPaths => [IconResPath];

    public override async Task<bool> OnSelect()
    {
        var dishonorCards = Owner.Deck.Cards.OfType<Dishonor>().ToList();

        if (dishonorCards.Count == 0)
            return false;

        foreach (var dishonorCard in dishonorCards)
        {
            await CardPileCmd.RemoveFromDeck(dishonorCard, true);
        }

        return true;
    }
}
