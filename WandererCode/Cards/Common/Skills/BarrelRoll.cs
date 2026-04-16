using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using MegaCrit.Sts2.Core.HoverTips;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>shift</tags>
/// <art>wanderer mid-air about to land seemingly after a fuckin barrel roll</art>
/// <kanji>横転</kanji>
[Pool(typeof(WandererCardPool))]
public class BarrelRoll : WandererCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [WandererKeywords.ShiftHoverTip];

    public BarrelRoll() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (IsUpgraded)
        {
            await WandererCmd.PickAndShiftCardsFromHand(choiceContext, 1, Owner, this);
        }
        else
        {
            var pile = PileType.Hand.GetPile(Owner);
            var candidates = pile.Cards.Where(c => !c.Keywords.Contains(WandererKeywords.Enshrined)).ToList();
            var card = Owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
            if (card != null)
            {
                await WandererCmd.ShiftCard(card, Owner);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}