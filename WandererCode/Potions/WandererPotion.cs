using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Extensions;

namespace Wanderer.WandererCode.Potions;

[Pool(typeof(WandererPotionPool))]
public abstract class WandererPotion : CustomPotionModel
{
    public override string CustomPackedImagePath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
            return ResourceLoader.Exists(path) ? path : "potion.png".PotionImagePath();
        }
    }

    public override string CustomPackedOutlinePath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".PotionImagePath();
            return ResourceLoader.Exists(path) ? path : "potion.png".PotionImagePath();
        }
    }
}