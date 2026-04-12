using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Keywords;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>shift, energy</tags>
[Pool(typeof(WandererCardPool))]
public class Improvise : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ImprovisePower>(1m)];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [WandererKeywords.ShiftHoverTip];

    public Improvise() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ImprovisePower>(Owner.Creature, DynamicVars["ImprovisePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}