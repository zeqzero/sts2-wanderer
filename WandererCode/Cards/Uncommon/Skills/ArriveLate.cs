using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>aoe, weak, vuln</tags>
[Pool(typeof(WandererCardPool))]
public class ArriveLate : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Innate,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>(1m),
        new PowerVar<VulnerablePower>(1m)
    ];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    public ArriveLate() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<WeakPower>(enemy, DynamicVars.Weak.BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(enemy, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["WeakPower"].UpgradeValueBy(1m);
        base.DynamicVars["VulnerablePower"].UpgradeValueBy(1m);
    }
}