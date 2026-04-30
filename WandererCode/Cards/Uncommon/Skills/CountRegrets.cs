using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>death, exhaust</tags>
/// <art>wanderer face palming, tight zoom on head</art>
/// <kanji>悔</kanji>
[Pool(typeof(WandererCardPool))]
public class CountRegrets : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ WandererKeywords.Enshrined ];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [ HoverTipFactory.FromCard<Ofuda>() ];

    public CountRegrets() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int openSlots = 10 - PileType.Hand.GetPile(Owner).Cards.Count();
        var ofudas = new List<CardModel>();
        for (int i = 0; i < openSlots; i++)
        {
            ofudas.Add(CombatState.CreateCard<Ofuda>(Owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(ofudas, PileType.Hand, addedByPlayer: true);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
