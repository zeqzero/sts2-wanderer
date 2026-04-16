using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Cards;

/// <tags>dance, commit</tags>
/// <art>kendo vitruvian man, all five Kamae shown overlayed atop eachother, Hasso highlighted, halo around head</art>
/// <kanji>無構</kanji>
[Pool(typeof(WandererCardPool))]
public class MuGamae : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Times", 2m)
    ];

    public MuGamae() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    private CardModel CreateChoiceCard<T>() where T : CardModel, new()
    {
        var card = (CardModel)ModelDb.Card<T>().MutableClone();
        card.Owner = Owner;
        return card;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> cards = [];

        // Include all stances, even current one
        if (WandererCmd.JodanEnabled)
            cards.Add(CreateChoiceCard<EnterJodan>());
        cards.Add(CreateChoiceCard<EnterChudan>());
        cards.Add(CreateChoiceCard<EnterHasso>());
        cards.Add(CreateChoiceCard<EnterGedan>());
        if (WandererCmd.WakiEnabled)
            cards.Add(CreateChoiceCard<EnterWaki>());

        if (cards.Count == 0) return;

        CardModel? chosenCard = await WandererCmd.ChooseCard(choiceContext, cards, Owner, canSkip: false);
        if (chosenCard is IEnterStance enterStance)
        {
            await enterStance.OnEnter(choiceContext, cardPlay, DynamicVars["Times"].IntValue);
        }
    }

    // At the start of your turn, put this card in your hand.
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        var pile = PileType.Draw.GetPile(Owner).Cards.FirstOrDefault(c => c == this)
            ?? PileType.Discard.GetPile(Owner).Cards.FirstOrDefault(c => c == this);

        if (pile != null)
        {
            await CardPileCmd.Add(pile, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Times"].UpgradeValueBy(1m);
    }
}
