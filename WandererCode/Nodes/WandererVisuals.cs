using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Wanderer.WandererCode.Nodes;

public static class WandererVisuals
{
    private static readonly Dictionary<string, Texture2D> _stanceTextures = new();

    private static readonly string[] Stances = ["chudan", "gedan", "hasso", "jodan", "waki"];

    public static void SetStance(Creature creature, string stance)
    {
        var sprite = GetSprite(creature);
        if (sprite == null) return;

        if (_stanceTextures.Count == 0)
            LoadTextures();

        if (_stanceTextures.TryGetValue(stance, out var tex))
            sprite.Texture = tex;
        else
            GD.PrintErr($"[WandererVisuals] Unknown stance: {stance}");
    }

    private static Sprite2D? GetSprite(Creature creature)
    {
        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
        var body = creatureNode?.Visuals?.Body;
        return body?.GetNodeOrNull<Sprite2D>("Sprite");
    }

    private static void LoadTextures()
    {
        foreach (var stance in Stances)
        {
            var tex = GD.Load<Texture2D>($"res://Wanderer/images/wanderer/{stance}.png");
            if (tex != null)
                _stanceTextures[stance] = tex;
            else
                GD.PrintErr($"[WandererVisuals] Failed to load texture for stance: {stance}");
        }
    }
}
