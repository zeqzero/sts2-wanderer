using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>dishonor, shift</tags>
/// <art></art>
[Pool(typeof(WandererCardPool))]
public class Spit : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ CardKeyword.Exhaust ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [ new PowerVar<WeakPower>(99), new CardsVar(3) ];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [ WandererKeywords.ShiftHoverTip, HoverTipFactory.FromCard<Dishonor>() ];

    public Spit() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<WeakPower>(cardPlay.Target, DynamicVars["WeakPower"].BaseValue, Owner.Creature, this);
        await WandererCmd.PickAndShiftCardsFromHand(choiceContext, (int)DynamicVars.Cards.BaseValue, Owner, this, upgrade: IsUpgraded);
        await WandererCmd.AddDishonor(Owner, CombatState);
    }
}