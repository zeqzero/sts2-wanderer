using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.sts2.Core.Nodes.TopBar;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Patches;

[HarmonyPatch]
internal class NTopBarHp_Patches
{
    [HarmonyPatch(typeof(NTopBarHp), "OnFocus")]
    private static class OnFocusPatch
    {
        private static readonly FieldInfo PlayerField =
            AccessTools.Field(typeof(NTopBarHp), "_player");

        private static readonly HoverTip BaseHpTip = new(
            new LocString("static_hover_tips", "HIT_POINTS.title"),
            new LocString("static_hover_tips", "HIT_POINTS.description"));

        private static bool Prefix(NTopBarHp __instance)
        {
            var player = PlayerField.GetValue(__instance) as Player;
            if (player?.Character is not Character.Wanderer)
                return true;

            var desc = new LocString("static_hover_tips", "WANDERER-SHINIGAMI_HP.description");
            desc.Add("Current", WandererCmd.GetShinigamiCurrentHp(player.Creature));
            desc.Add("Max", WandererCmd.GetShinigamiMaxHp(player.Creature));
            var shinigamiTip = new HoverTip(
                new LocString("static_hover_tips", "WANDERER-SHINIGAMI_HP.title"),
                desc);

            var tips = new List<IHoverTip> { BaseHpTip, shinigamiTip };
            var set = NHoverTipSet.CreateAndShow(__instance, tips);
            set.GlobalPosition = __instance.GlobalPosition + new Vector2(0f, __instance.Size.Y + 20f);
            return false;
        }
    }
}
