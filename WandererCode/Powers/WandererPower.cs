using BaseLib.Abstracts;
using BaseLib.Extensions;
using Wanderer.WandererCode.Extensions;
using Godot;
using Wanderer.WandererCode.Interfaces;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Wanderer.WandererCode.Powers;

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
}