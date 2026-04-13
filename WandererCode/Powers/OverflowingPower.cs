using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Powers;

/// <art>copy Slippery</art>
public class OverflowingPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player && cardPlay.Card.Type == CardType.Attack)
        {
            foreach (var _ in Enumerable.Range(0, Amount))
            {
                var randomStance = WandererCmd.GetRandomStance(Owner, true);
                if (randomStance != null)
                {
                    await WandererCmd.EnterStance(Owner, (Stance)randomStance, 1);
                }
            }
        }
    }
}