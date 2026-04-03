using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Cards;

/// <tags>aoe, flurry, stance</tags>
[Pool(typeof(WandererCardPool))]
public class Flourish : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [ 
        new DamageVar(5m, ValueProp.Move)
    ];

    public Flourish() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(StanceCmd.GetShiftCount(Owner.Creature)).FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitVfxNode((Creature t) => NStabVfx.Create(t, facingEnemies: true, VfxColor.Gold))
            .SpawningHitVfxOnEachCreature()
            .Execute(choiceContext);
    }
}