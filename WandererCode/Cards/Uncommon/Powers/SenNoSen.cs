using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>flurry</tags>
/// <art>wanderer in chudan stance, action lines indicating parries, viewed from unique angle</art>
[Pool(typeof(WandererCardPool))]
public class SenNoSen : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SenNoSenPower>(2)];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromPower<SenNoSenPower>()];

    public SenNoSen() : base(3, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SenNoSenPower>(Owner.Creature, DynamicVars["SenNoSenPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SenNoSenPower"].UpgradeValueBy(1m);
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this || IsClone)
            return Task.CompletedTask;

        int attacks = CombatManager.Instance.History.CardPlaysFinished
            .Count(e => e.CardPlay.Card.Type == CardType.Attack
                     && e.CardPlay.Card.Owner == Owner
                     && e.HappenedThisTurn(CombatState));
        EnergyCost.AddThisTurn(-attacks);
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || cardPlay.Card.Type != CardType.Attack)
            return Task.CompletedTask;

        EnergyCost.AddThisTurn(-1);
        return Task.CompletedTask;
    }
}
