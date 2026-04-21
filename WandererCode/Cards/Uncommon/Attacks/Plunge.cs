using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>draw, counter</tags>
/// <art>wanderer after sticking their katana all the way through an enemy, only the hilt is seen, the end of the katana visible on the other side of the foe</art>
/// <kanji>貫</kanji>
[Pool(typeof(WandererCardPool))]
public class Plunge : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ new DamageVar(6m, ValueProp.Move) ];

    public Plunge() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        CardModel? drawn;
        do
        {
            drawn = await CardPileCmd.Draw(choiceContext, Owner);
        }
        while (drawn != null && drawn.Type == CardType.Skill && CardPile.GetCards(Owner, PileType.Hand).Count() < 10);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}