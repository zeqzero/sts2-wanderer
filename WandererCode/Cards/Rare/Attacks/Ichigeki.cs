using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Cards;

/// <tags>commit</tags>
/// <art>wanderer sheathing sword, single big action line indicating instantaneous high power attack</art>
[Pool(typeof(WandererCardPool))]
public class Ichigeki : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ new DamageVar(20m, ValueProp.Move), new DynamicVar("AdditionalDamage", 7) ];

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
        var damage = DynamicVars.Damage.BaseValue + DynamicVars["AdditionalDamage"].BaseValue * _turnsInCurrentStance;
        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AdditionalDamage"].UpgradeValueBy(3);
    }
}