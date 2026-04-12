using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>dishonor</tags>
[Pool(typeof(WandererCardPool))]
public class SecretSauce : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ new PowerVar<SecretSaucePower>(1m) ];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [ HoverTipFactory.FromCard<Dishonor>() ];

    public SecretSauce() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SecretSaucePower>(Owner.Creature, DynamicVars["SecretSaucePower"].BaseValue, Owner.Creature, this);
        await WandererCmd.AddDishonor(Owner, CombatState);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}