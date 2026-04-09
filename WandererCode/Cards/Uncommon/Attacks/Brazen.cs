using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>flurry</tags>
[Pool(typeof(WandererCardPool))]
public class Brazen : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

    public Brazen() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(3).FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash", null, "slash_attack.mp3")
            .Execute(choiceContext);

        var drawPile = PileType.Draw.GetPile(Owner);
        var nonAttacks = drawPile.Cards.Where(c => c.Type != CardType.Attack).ToList();
        var toDiscard = new List<CardModel>();
        for (int i = 0; i < 5 && nonAttacks.Count > 0; i++)
        {
            var card = Owner.RunState.Rng.CombatCardSelection.NextItem(nonAttacks)!;
            nonAttacks.Remove(card);
            toDiscard.Add(card);
        }

        if (toDiscard.Count > 0)
            await CardCmd.Discard(choiceContext, toDiscard);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
