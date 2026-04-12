using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Wanderer.WandererCode.Powers;

public class KiaiPower : WandererPower, IWandererNextTurnPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player) return;

        int uniqueNextTurnPowers = Owner.Powers
            .Where(p => p is IWandererNextTurnPower && p.Amount != 0)
            .Select(p => p.GetType())
            .Distinct()
            .Count();

        if (uniqueNextTurnPowers > 0)
        {
            Flash();
            int totalDraw = Amount * uniqueNextTurnPowers;
            int totalStrength = 2 * Amount * uniqueNextTurnPowers;

            await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), totalDraw, player);
            await PowerCmd.Apply<StrengthPower>(Owner, totalStrength, Owner, null);
        }

        await PowerCmd.Remove(this);
    }

    public async Task ApplyNow(PlayerChoiceContext choiceContext, Player player)
    {
        int uniqueNextTurnPowers = Owner.Powers
            .Where(p => p is IWandererNextTurnPower && p.Amount != 0)
            .Select(p => p.GetType())
            .Distinct()
            .Count();

        if (uniqueNextTurnPowers > 0)
        {
            Flash();
            int totalDraw = Amount * uniqueNextTurnPowers;
            int totalStrength = 2 * Amount * uniqueNextTurnPowers;

            await CardPileCmd.Draw(choiceContext, totalDraw, player);
            await PowerCmd.Apply<StrengthPower>(Owner, totalStrength, Owner, null);
        }

        await PowerCmd.Remove(this);
    }
}
