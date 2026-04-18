using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>dance, commit</tags>
/// <art>kendo vitruvian man, chudan hasso and gedan shown, chudan highlighted</art>
/// <kanji>構</kanji>
[Pool(typeof(WandererCardPool))]
public class Kamae : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    public Kamae() : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
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

        if (!Owner.Creature.Powers.OfType<JodanPower>().Any() && WandererCmd.IsJodanEnabled(Owner.Creature))
            cards.Add(CreateChoiceCard<EnterJodan>());
        if (!Owner.Creature.Powers.OfType<ChudanPower>().Any())
            cards.Add(CreateChoiceCard<EnterChudan>());
        if (!Owner.Creature.Powers.OfType<HassoPower>().Any())
            cards.Add(CreateChoiceCard<EnterHasso>());
        if (!Owner.Creature.Powers.OfType<GedanPower>().Any())
            cards.Add(CreateChoiceCard<EnterGedan>());
        if (!Owner.Creature.Powers.OfType<WakiPower>().Any() && WandererCmd.IsWakiEnabled(Owner.Creature))
            cards.Add(CreateChoiceCard<EnterWaki>());

        if (cards.Count == 0) return;

        CardModel? chosenCard = await WandererCmd.ChooseCard(choiceContext, cards, Owner, canSkip: false);
        if (chosenCard is IEnterStance enterStance)
        {
            await enterStance.OnEnter(choiceContext, cardPlay, 1);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}