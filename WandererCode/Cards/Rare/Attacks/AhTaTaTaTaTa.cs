using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>flurry, xcost</tags>
/// <art>wanderer making dozens of attacks simultaneous, multiple katanas visible through a cloud of action lines</art>
/// <kanji>乱打</kanji>
[Pool(typeof(WandererCardPool))]
public class AhTaTaTaTaTa : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ new DamageVar(1m, ValueProp.Move), new DynamicVar("Hits", 5), new DynamicVar("Replay", 1) ];

    public AhTaTaTaTaTa() : base(3, CardType.Attack, CardRarity.Rare, TargetType.RandomEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState, "CombatState");

        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars["Hits"].IntValue)
            .FromCard(this)
            .TargetingRandomOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        BaseReplayCount += DynamicVars["Replay"].IntValue;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Replay"].UpgradeValueBy(1);
    }
}