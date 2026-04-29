using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Powers;

/// <art>waterfall with shield overlay</art>
public class FlowPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterStanceLeft(Creature creature, Stance stance)
    {
        if (creature != Owner || Owner.Player == null)
            return;

        var player = Owner.Player;
        var costReduction = (int)Amount;

        await RunAsHookAction(async ctx =>
        {
            var candidates = PileType.Hand.GetPile(player).Cards.Where(c => c.EnergyCost.GetResolved() > 0).ToList();
            if (candidates.Count == 0)
                return;

            var target = player.RunState.Rng.CombatCardSelection.NextItem(candidates);
            target.EnergyCost.AddThisTurnOrUntilPlayed(-costReduction);
        });
    }
}
