using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>steady</tags>
/// <art>wanderer sheathing sword, single big action line indicating instantaneous high power attack</art>
/// <kanji>一撃</kanji>
[Pool(typeof(WandererCardPool))]
public class Ichigeki : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(12m),
        new ExtraDamageVar(3m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (CardModel card, Creature? _) =>
            card.Owner.Creature.GetPower<SteadyPower>()?.Amount ?? 0m)
    ];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromPower<SteadyPower>()];

    public Ichigeki() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
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
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}
