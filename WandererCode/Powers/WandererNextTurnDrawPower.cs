using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Powers;

/// <art></art>
public sealed class WandererNextTurnDrawPower : WandererPower, IWandererNextTurnPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => true;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner.Player)
        {
            return count;
        }

        if (AmountOnTurnStart == 0)
        {
            return count;
        }

        return count + Amount;
    }

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == Owner.Side && AmountOnTurnStart != 0 && !WandererCmd.ShouldPreserveNextTurnPowers(Owner))
        {
            if (Amount == AmountOnTurnStart)
            {
                await PowerCmd.Remove(this);
            }
            else
            {
                await PowerCmd.Apply(this, Owner, -AmountOnTurnStart, Owner, null);
            }
        }
    }

    public async Task ApplyNow(PlayerChoiceContext choiceContext, Player player)
    {
        if (Amount >= 0)
        {
            await CardPileCmd.Draw(choiceContext, Amount, player);
        }
        else
        {
            CardPile pile = PileType.Hand.GetPile(player);
            for (int i = 0; i < Amount; i++)
            {
                var randomCard = Owner.Player.RunState.Rng.CombatCardSelection.NextItem(pile.Cards);
                if (randomCard != null)
                {
                    await CardCmd.Discard(choiceContext, randomCard);
                }
            }
        }

        if (!WandererCmd.ShouldPreserveNextTurnPowers(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}