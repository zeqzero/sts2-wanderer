using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Cards;

/// <tags>commit</tags>
[Pool(typeof(WandererCardPool))]
public class RootedStrike : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ new DamageVar(7m, ValueProp.Move), new DynamicVar("ReenterAmount", 1) ];

    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

    public RootedStrike() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var stancePower = WandererCmd.GetCurrentStancePower(Owner.Creature);
        if (stancePower is PowerModel power)
        {
            await WandererCmd.EnterStance(Owner.Creature, stancePower.Stance, (int)DynamicVars["ReenterAmount"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ReenterAmount"].UpgradeValueBy(1);
    }
}