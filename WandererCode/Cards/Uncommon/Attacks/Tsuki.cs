using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>flurry</tags>
[Pool(typeof(WandererCardPool))]
public class Tsuki : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedHits").WithMultiplier(delegate(CardModel card, Creature? _)
        {
            CardModel card2 = card;
            return 1 + CombatManager.Instance.History.Entries.OfType<CreatureAttackedEntry>().Count((CreatureAttackedEntry e) => e.Actor == card2.Owner.Creature && e.HappenedThisTurn(card2.CombatState));
        })
    ];

    public Tsuki() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitCount((int)((CalculatedVar)DynamicVars["CalculatedHits"]).Calculate(cardPlay.Target))
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }
}