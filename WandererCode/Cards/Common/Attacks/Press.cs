using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>dance</tags>
[Pool(typeof(WandererCardPool))]
public class Press : WandererCard
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Kamae>()];

    public Press() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // Check if Kamae is already in hand
        if (PileType.Hand.GetPile(Owner).Cards.Any(c => c is Kamae))
            return;

        // Search draw, discard, and exhaust for an existing Kamae
        CardModel? kamae = PileType.Draw.GetPile(Owner).Cards.FirstOrDefault(c => c is Kamae)
            ?? PileType.Discard.GetPile(Owner).Cards.FirstOrDefault(c => c is Kamae)
            ?? PileType.Exhaust.GetPile(Owner).Cards.FirstOrDefault(c => c is Kamae);

        if (kamae == null)
        {
            kamae = CombatState.CreateCard<Kamae>(Owner);
        }
        await CardPileCmd.Add(kamae, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}