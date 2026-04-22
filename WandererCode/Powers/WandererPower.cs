using BaseLib.Abstracts;
using BaseLib.Extensions;
using Wanderer.WandererCode.Extensions;
using Godot;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Interfaces;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

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
    // Defers the choice to its own GenericHookGameAction so the triggering action —
    // typically a PlayCardAction for a Power card, whose NCard is on a fly-VFX kill-timer —
    // can finish cleanly before the choice UI opens.
    protected async Task RunAsHookAction(Func<PlayerChoiceContext, Task> work)
    {
        if (!LocalContext.NetId.HasValue)
            return;

        var hookContext = new HookPlayerChoiceContext(this, LocalContext.NetId.Value, CombatState, GameActionType.Combat);
        await hookContext.AssignTaskAndWaitForPauseOrCompletion(work(hookContext));
    }
}