using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>commit, exhaust</tags>
/// <art>wanderer exhaling smoke from a long thin pipe, zoomed-in on face</art>
/// <kanji>不動心</kanji>
[Pool(typeof(WandererCardPool))]
public class Fudoshin : WandererCard
{
    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromPower<FudoshinPower>()];

    public Fudoshin() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var piles = new[] { PileType.Hand, PileType.Draw, PileType.Discard };
        foreach (var pile in piles)
        {
            foreach (var card in pile.GetPile(Owner).Cards.Where(c => c is Kamae).ToList())
            {
                await CardCmd.Exhaust(choiceContext, card);
            }
        }

        await PowerCmd.Apply<FudoshinPower>(Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
