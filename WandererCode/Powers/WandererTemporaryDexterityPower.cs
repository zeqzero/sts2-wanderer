using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Models.Powers;
using Wanderer.WandererCode.Extensions;

namespace Wanderer.WandererCode.Powers;

/// <art>copy Temporary Dexterity</art>
public abstract class WandererTemporaryDexterityPower : TemporaryDexterityPower, ICustomPower, ICustomModel
{
    string? ICustomPower.CustomPackedIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
        }
    }

    string? ICustomPower.CustomBigIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".BigPowerImagePath();
        }
    }
}
