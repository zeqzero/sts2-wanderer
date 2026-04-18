using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Cards;

/// <tags>commit</tags>
/// <art>wanderer sheathing sword, single big action line indicating instantaneous high power attack</art>
/// <kanji>一撃</kanji>
[Pool(typeof(WandererCardPool))]
public class Ichigeki : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(20m),
        new ExtraDamageVar(7m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (CardModel card, Creature? _) =>
            ((Ichigeki)card)._turnsInCurrentStance)
    ];

    private int _turnsInCurrentStance = 0;

    public Ichigeki() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override async Task AfterStanceEntered(Creature creature, Stance stance)
    {
        if (creature == Owner.Creature)
        {
            _turnsInCurrentStance = 1;
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            var stance = WandererCmd.GetCurrentStancePower(Owner.Creature);
            if (stance != null)
            {
                _turnsInCurrentStance++;
            }
        }
    }

    public override async Task AfterStanceLeft(Creature creature, Stance oldStance)
    {
        if (creature == Owner.Creature)
        {
            _turnsInCurrentStance = 0;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(3m);
    }
}