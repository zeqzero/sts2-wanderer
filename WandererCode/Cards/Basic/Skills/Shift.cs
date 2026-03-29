using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

[Pool(typeof(WandererCardPool))]
public class Shift : WandererCard
{
    public Shift() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }

    private CardModel CreateChoiceCard<T>() where T : CardModel, new()
    {
        var card = (CardModel)ModelDb.Card<T>().MutableClone();
        card.Owner = base.Owner;
        return card;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> cards = [];

        if (!Owner.Creature.Powers.OfType<ChudanPower>().Any())
            cards.Add(CreateChoiceCard<ShiftChudan>());
        if (!Owner.Creature.Powers.OfType<HassoPower>().Any())
            cards.Add(CreateChoiceCard<ShiftHasso>());
        if (!Owner.Creature.Powers.OfType<GedanPower>().Any())
            cards.Add(CreateChoiceCard<ShiftGedan>());

        if (cards.Count == 0) return;

        CardModel? chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, Owner, canSkip: false);
        if (chosenCard is IShiftStance shiftStance)
        {
            await shiftStance.OnShift(choiceContext, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}