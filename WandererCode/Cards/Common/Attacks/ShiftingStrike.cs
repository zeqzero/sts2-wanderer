using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

[Pool(typeof(WandererCardPool))]
public class ShiftingStrike : WandererCard
{
    protected override HashSet<CardTag> CanonicalTags => [ CardTag.Strike ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [ new DamageVar(9m, ValueProp.Move) ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ HoverTipFactory.FromCard<Shift>() ];

    public ShiftingStrike() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // Check if Shift is already in hand
        if (PileType.Hand.GetPile(Owner).Cards.Any(c => c is Shift))
            return;

        // Search draw, discard, and exhaust for an existing Shift
        CardModel? shift = PileType.Draw.GetPile(Owner).Cards.FirstOrDefault(c => c is Shift)
            ?? PileType.Discard.GetPile(Owner).Cards.FirstOrDefault(c => c is Shift)
            ?? PileType.Exhaust.GetPile(Owner).Cards.FirstOrDefault(c => c is Shift);

        if (shift == null)
        {
            shift = CombatState.CreateCard<Shift>(Owner);
        }
        await CardPileCmd.Add(shift, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}