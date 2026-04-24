using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Wanderer.WandererCode.Powers;

/// <summary>
/// Bridges Keikaku / Preempt / Kime to base-game next-turn powers so Wanderer can
/// pick up cards like Relax and still have their effects doubled / triggered / counted.
/// Base-game next-turn powers are sealed, so we dispatch by type instead of an interface.
/// Jigoku Junbi's preserve-next-turn behavior does not extend to base-game powers.
/// </summary>
public static class NextTurnPowers
{
    public static bool Is(PowerModel power) =>
        power is IWandererNextTurnPower
        || power is BlockNextTurnPower
        || power is EnergyNextTurnPower
        || power is StarNextTurnPower
        || power is DrawCardsNextTurnPower
        || power is SummonNextTurnPower;

    public static async Task ApplyNow(PowerModel power, PlayerChoiceContext choiceContext, Player player)
    {
        if (power is IWandererNextTurnPower wanderer)
        {
            await wanderer.ApplyNow(choiceContext, player);
            return;
        }

        switch (power)
        {
            case BlockNextTurnPower:
                await CreatureCmd.GainBlock(power.Owner, power.Amount, ValueProp.Unpowered, null);
                break;
            case EnergyNextTurnPower:
                await PlayerCmd.GainEnergy(power.Amount, player);
                break;
            case StarNextTurnPower:
                await PlayerCmd.GainStars(power.Amount, player);
                break;
            case DrawCardsNextTurnPower:
                await CardPileCmd.Draw(choiceContext, power.Amount, player);
                break;
            case SummonNextTurnPower:
                await OstyCmd.Summon(choiceContext, player, power.Amount, power);
                break;
            default:
                return;
        }

        await PowerCmd.Remove(power);
    }
}
