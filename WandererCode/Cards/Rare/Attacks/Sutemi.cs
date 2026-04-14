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

/// <tags>exhaust, nextturn</tags>
/// <art>wanderer after completing an attack, cut up enemy with blood just beginning to spray from the wounds</art>
[Pool(typeof(WandererCardPool))]
public class Sutemi : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new DynamicVar("NextTurnDamage", 3)
    ];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WandererNextTurnDamagePower>()
    ];

    public Sutemi() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash", null, "slash_attack.mp3")
            .Execute(choiceContext);

        int exhaustCount = PileType.Exhaust.GetPile(Owner).Cards.Count;
        int nextTurnDamage = DynamicVars["NextTurnDamage"].IntValue * exhaustCount;
        if (nextTurnDamage > 0)
        {
            await PowerCmd.Apply<WandererNextTurnDamagePower>(Owner.Creature, nextTurnDamage, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NextTurnDamage"].UpgradeValueBy(1m);
    }
}
