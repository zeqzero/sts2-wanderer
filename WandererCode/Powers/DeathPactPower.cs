using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Wanderer.WandererCode.Powers;

/// <art></art>
public class DeathPactPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public Creature? Source { get; set; }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Source) return;

        Flash();
        await PowerCmd.Apply<IntangiblePower>(Owner, Amount, Source, null);
        await PowerCmd.Remove(this);
    }

    public override async Task BeforeRitualDeath(Creature creature)
    {
        if (creature != Source) return;

        Flash();
        await PowerCmd.Apply<IntangiblePower>(Owner, Amount, Source, null);
        await PowerCmd.Remove(this);
    }

}
