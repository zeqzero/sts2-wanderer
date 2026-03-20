using BaseLib.Abstracts;
using BaseLib.Utils;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Potions;

[Pool(typeof(WandererPotionPool))]
public abstract class WandererPotion : CustomPotionModel;