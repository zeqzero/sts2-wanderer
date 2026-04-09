using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Wanderer.WandererCode.Powers;

public class SenNoSenPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player && cardPlay.Card.Type == CardType.Attack)
        {
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
        }
    }
}
