using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Wanderer.WandererCode.Cards;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Commands;

public static class ShinigamiCmd
{
    private const int ShinigamiMaxHp = 5;
    private const int ExhaustThreshold = 5;
    private static readonly Color ShinigamiTint = new(Colors.White, 0.3f);

    public static HoverTip CanonicalPowerHoverTip
    {
        get
        {
            var desc = new LocString("powers", "WANDERER-SHINIGAMI_POWER.description");
            desc.Add("Amount", ExhaustThreshold);
            return new HoverTip(ModelDb.Power<ShinigamiPower>(), desc.GetFormattedText(), false);
        }
    }

    /// <summary>
    /// Per-creature state for shinigami form. Tracks HP, max HP,
    /// tint, and player reference independently for each creature.
    /// </summary>
    private class ShinigamiState
    {
        public bool Active;
        public decimal? StoredHp;
        public int PreShinigamiMaxHp;
        public decimal ShinigamiCurrentHp = ShinigamiMaxHp;
        public Color? OriginalModulate;
        public Player? Player;
    }

    private static readonly Dictionary<Creature, ShinigamiState> _states = new();

    /// <summary>
    /// Maps Ofuda → backup clone of the original card it replaced.
    /// Backups are created via CloneCard before transforming, so they remain
    /// properly registered in CombatState and can be transformed back cleanly.
    /// Shared across all players since ofuda instances are unique keys.
    /// </summary>
    private static readonly Dictionary<CardModel, CardModel> _transformedCards = new();

    private static ShinigamiState GetOrCreateState(Creature creature)
    {
        if (!_states.TryGetValue(creature, out var state))
        {
            state = new ShinigamiState();
            _states[creature] = state;
        }
        return state;
    }

    public static bool InShinigamiForm(Creature creature)
    {
        return _states.TryGetValue(creature, out var state) && state.Active;
    }

    public static void SetStoredHp(Creature creature, decimal hp)
    {
        GetOrCreateState(creature).StoredHp = hp;
    }

    public static CardModel? GetOriginalCard(CardModel ofudaCard)
    {
        return _transformedCards.GetValueOrDefault(ofudaCard);
    }

    public static void RemoveTransformEntry(CardModel ofudaCard)
    {
        _transformedCards.Remove(ofudaCard);
    }

    /// <summary>
    /// Transforms a single card into an Ofuda, storing a backup clone.
    /// Called by ShinigamiPower hooks to catch cards that arrive mid-shinigami-form.
    /// </summary>
    public static async Task TransformCard(CardModel card)
    {
        var combatState = card.CombatState;
        var backup = combatState.CloneCard(card);
        var ofuda = combatState.CreateCard<Ofuda>(card.Owner);
        _transformedCards[ofuda] = backup;
        await CardCmd.Transform(card, ofuda);
    }

    public static async Task EnterShinigamiForm(Creature creature, Player player)
    {
        var state = GetOrCreateState(creature);
        if (state.Active) return;

        state.PreShinigamiMaxHp = creature.MaxHp;
        state.Player = player;
        state.Active = true;

        creature.SetMaxHpInternal(ShinigamiMaxHp);
        await CreatureCmd.Heal(creature, state.ShinigamiCurrentHp);

        await PowerCmd.Apply<ShinigamiPower>(creature, ExhaustThreshold, creature, null);

        await TransformAllCards(player);

        ApplyShinigamiTint(creature, state);
    }

    public static async Task ExitShinigamiForm(Creature creature)
    {
        if (!_states.TryGetValue(creature, out var state) || !state.Active) return;

        state.Active = false;
        state.ShinigamiCurrentHp = creature.CurrentHp;

        await RestoreAllCards(state.Player);

        creature.SetMaxHpInternal(state.PreShinigamiMaxHp);

        decimal healTo = state.StoredHp ?? 1m;
        await CreatureCmd.Heal(creature, healTo);

        state.StoredHp = null;

        await PowerCmd.Remove<ShinigamiPower>(creature);

        ResetShinigamiTint(creature, state);
    }

    private static async Task TransformAllCards(Player player)
    {
        var allCards = new List<CardModel>();
        allCards.AddRange(PileType.Hand.GetPile(player).Cards);
        allCards.AddRange(PileType.Draw.GetPile(player).Cards);
        allCards.AddRange(PileType.Discard.GetPile(player).Cards);

        foreach (var card in allCards)
        {
            await TransformCard(card);
        }
    }

    private static async Task RestoreAllCards(Player? player)
    {
        if (player == null) return;

        // Collect only the ofuda belonging to this player
        var toRestore = _transformedCards
            .Where(kvp => kvp.Key.Owner == player)
            .ToList();

        if (toRestore.Count == 0) return;

        foreach (var (ofuda, backup) in toRestore)
        {
            await CardCmd.Transform(ofuda, backup);
            _transformedCards.Remove(ofuda);
        }
    }

    private static void ApplyShinigamiTint(Creature creature, ShinigamiState state)
    {
        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (creatureNode == null) return;

        var body = creatureNode.Body;
        state.OriginalModulate ??= body.Modulate;
        body.Modulate = ShinigamiTint;
    }

    private static void ResetShinigamiTint(Creature creature, ShinigamiState state)
    {
        if (state.OriginalModulate == null) return;

        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (creatureNode == null) return;

        creatureNode.Body.Modulate = state.OriginalModulate.Value;
        state.OriginalModulate = null;
    }

    /// <summary>
    /// Call at the start of each combat to reset state.
    /// </summary>
    public static void Reset()
    {
        _states.Clear();
        _transformedCards.Clear();
    }
}
