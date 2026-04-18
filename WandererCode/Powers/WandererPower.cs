using BaseLib.Abstracts;
using BaseLib.Extensions;
using Wanderer.WandererCode.Extensions;
using Godot;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Interfaces;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Wanderer.WandererCode.Powers;

/// <art></art>
public abstract class WandererPower : CustomPowerModel, IWandererEventListener
{
    //Loads from Wanderer/images/powers/your_power.png
    public override string CustomPackedIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
        }
    }

    public override string CustomBigIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".BigPowerImagePath();
        }
    }

    public virtual async Task AfterEnteredShinigami(Creature creature)
    {
    }

    public virtual async Task BeforeRitualDeath(Creature creature)
    {
    }

    public virtual async Task AfterStanceLeft(Creature creature, Stance oldStance)
    {
    }

    public virtual async Task AfterStanceEntered(Creature creature, Stance stance)
    {
    }

    public virtual async Task AfterShifted(CardModel card)
    {
    }

    public virtual async Task AfterRefilled(CardModel card)
    {
    }

    // For hooks that don't plumb a PlayerChoiceContext (AfterPowerAmountChanged, etc.).
    // Piggybacks on the currently-running action so remote clients pause for the choice
    // via the same mechanism PlayCardAction uses for its own OnPlay choices.
    protected async Task RunWithChoiceContext(Func<PlayerChoiceContext, Task> work)
    {
        var action = RunManager.Instance.ActionExecutor.CurrentlyRunningAction;
        PlayerChoiceContext context = action != null
            ? new GameActionPlayerChoiceContext(action)
            : new BlockingPlayerChoiceContext();
        await work(context);
    }
}