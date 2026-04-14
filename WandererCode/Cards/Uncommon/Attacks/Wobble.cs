using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Cards;

/// <tags>shift, refill</tags>
/// <art>wanderer swinging a slung gourd up in order to pour from it, foe accidentally hit in the process falling backward, action lines</art>
[Pool(typeof(WandererCardPool))]
public class Wobble : WandererCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        new BlockVar(10m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [WandererKeywords.Refills];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips =>
    [
        WandererKeywords.ShiftHoverTip
    ];

    public Wobble() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash", null, "slash_attack.mp3")
            .Execute(choiceContext);
    }

    // Shift self after it lands in a post-play pile
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card == this && oldPileType == PileType.Play && card.Owner == Owner)
        {
            await WandererCmd.ShiftCard(this, Owner);
        }
    }

    // Fires on the restored Wobble instance after the second shift reverts it via Refill.
    public override async Task AfterRefilled(CardModel card)
    {
        if (card == this && CombatState != null)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}
