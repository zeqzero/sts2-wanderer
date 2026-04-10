using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.RestSite;

namespace Wanderer.WandererCode.Patches;

[HarmonyPatch]
internal class RestSiteOption_Patches
{
    // RestSiteOption.Icon is hard-coded to res://images/ui/rest_site/option_<id>.png,
    // which a mod PCK can't populate (mods can only contribute files under their own
    // mod-id subtree). Intercept the getter for our custom options and load the icon
    // from the mod's own asset folder instead.
    [HarmonyPatch(typeof(RestSiteOption), "Icon", MethodType.Getter)]
    private static class IconGetterPatch
    {
        private static bool Prefix(RestSiteOption __instance, ref Texture2D __result)
        {
            if (__instance is not MisogiRestSiteOption)
                return true;

            __result = PreloadManager.Cache.GetTexture2D(MisogiRestSiteOption.IconResPath);
            return false;
        }
    }
}
