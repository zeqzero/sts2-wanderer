using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace Wanderer.WandererCode.Patches;

// Skip Spine animation playback on merchant scenes whose root child isn't a SpineSprite
// (the Wanderer merchant is a static Sprite2D, not a Spine rig). Without this, _Ready
// spams "Expected BoundObject to be a SpineSprite, but it is a Sprite2D!" every time
// the merchant appears.
[HarmonyPatch]
internal class WandererMerchant_Patches
{
    [HarmonyPatch(typeof(NMerchantCharacter), nameof(NMerchantCharacter.PlayAnimation))]
    private static class SkipPlayAnimationWithoutSpinePatch
    {
        private static bool Prefix(NMerchantCharacter __instance)
        {
            if (__instance.GetChildCount() == 0) return false;
            return __instance.GetChild(0).GetClass() == "SpineSprite";
        }
    }
}
