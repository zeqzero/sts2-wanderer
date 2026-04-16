using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Cards;

/// <tags>shift, aoe</tags>
/// <art>drunken wanderer spinning wildly, concerned enemy(s) look on, action lines indicating chaotic spinning</art>
/// <kanji>旋</kanji>
[Pool(typeof(WandererCardPool))]
public class TheSpins : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move), new DynamicVar("AdditionalDamage", 1)];

    public TheSpins() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);
        var damage = DynamicVars.Damage.BaseValue + DynamicVars["AdditionalDamage"].BaseValue * WandererCmd.GetShiftCount(Owner.Creature);
        await DamageCmd.Attack(damage)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AdditionalDamage"].UpgradeValueBy(1m);
    }
}
