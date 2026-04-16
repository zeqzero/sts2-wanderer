using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using MegaCrit.Sts2.Core.HoverTips;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>draw, shift</tags>
/// <art>wanderer drinking from a small cup, one hand raising cup to face (hidden beneath hat), gourd visible</art>
/// <kanji>酌</kanji>
[Pool(typeof(WandererCardPool))]
public class Swig : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new DynamicVar("Shift", 1)];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [WandererKeywords.ShiftHoverTip];

    public Swig() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await WandererCmd.PickAndShiftCardsFromHand(choiceContext, 1, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}