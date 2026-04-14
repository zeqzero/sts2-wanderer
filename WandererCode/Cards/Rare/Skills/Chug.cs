using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>dishonor, shift, draw, energy</tags>
/// <art>wanderer drinking from gourd greedily, holding gourd with both hands, sword visible on ground, liquid pouring down face and on clothes and ground</art>
[Pool(typeof(WandererCardPool))]
public class Chug : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ CardKeyword.Exhaust ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [ new CardsVar(3), new EnergyVar(3) ];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [ WandererKeywords.ShiftHoverTip, HoverTipFactory.FromCard<Dishonor>() ];

    public Chug() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        
        await WandererCmd.AddDishonor(Owner, CombatState);

        var pile = PileType.Hand.GetPile(Owner);
        var candidates = pile.Cards.Where(c => !c.Keywords.Contains(WandererKeywords.Enshrined)).ToList();
        foreach (var card in candidates)
        {
            await WandererCmd.ShiftCard(card, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}