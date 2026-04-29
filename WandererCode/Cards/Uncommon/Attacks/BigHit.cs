using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>steady, energy</tags>
/// <art>Wanderer swinging downward with a heavy attack, head is concealed by a large cloud of smoke</art>
/// <kanji>大撃</kanji>
[Pool(typeof(WandererCardPool))]
public class BigHit : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Move), new EnergyVar(1)];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromPower<SteadyPower>()];

    public BigHit() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash", null, "slash_attack.mp3")
            .Execute(choiceContext);

        var steady = Owner.Creature.GetPower<SteadyPower>();
        if (steady != null && steady.Amount > 0)
        {
            await PlayerCmd.GainEnergy(steady.Amount, Owner);
        }

        await PowerCmd.Apply<SteadyPower>(Owner.Creature, 2, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
