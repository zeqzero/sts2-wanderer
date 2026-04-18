using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>death, aoe, flurry</tags>
/// <art>wanderer doing a rude gesture, see Bras d'honneur, ideally with an offended foe nearby</art>
/// <kanji>曝恥</kanji>
[Pool(typeof(WandererCardPool))]
public class FlauntShame : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ WandererKeywords.Enshrined ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedHits").WithMultiplier(static (CardModel card, Creature? _) =>
            PileType.Hand.GetPile(card.Owner).Cards.Count(c => c.Type == CardType.Status))
    ];

    public FlauntShame() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState, "CombatState");

        var statusCards = PileType.Hand.GetPile(Owner).Cards.Where(c => c.Type == CardType.Status).ToList();

        foreach (var card in statusCards)
        {
            await CardCmd.Discard(choiceContext, card);
        }

        if (statusCards.Count > 0)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(statusCards.Count)
                .FromCard(this)
                .TargetingAllOpponents(CombatState)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}