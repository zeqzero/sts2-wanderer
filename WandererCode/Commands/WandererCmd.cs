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
using Wanderer.WandererCode.Interfaces;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Commands;

public static class WandererCmd
{
    private const int ShinigamiMaxHp = 5;
    private const int ExhaustThreshold = 5;
    private static readonly Color ShinigamiTint = new(Colors.White, 0.3f);

    public static HoverTip ShinigamiPowerCanonicalHoverTip
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

    private static readonly Dictionary<Creature, ShinigamiState> _shinigamiStates = new();

    /// <summary>
    /// Maps Ofuda → backup clone of the original card it replaced.
    /// Backups are created via CloneCard before transforming, so they remain
    /// properly registered in CombatState and can be transformed back cleanly.
    /// Shared across all players since ofuda instances are unique keys.
    /// </summary>
    private static readonly Dictionary<CardModel, CardModel> _ofudaTransformedCards = new();

    private static ShinigamiState GetOrCreateState(Creature creature)
    {
        if (!_shinigamiStates.TryGetValue(creature, out var state))
        {
            state = new ShinigamiState();
            _shinigamiStates[creature] = state;
        }
        return state;
    }

    public static bool InShinigamiForm(Creature creature)
    {
        return _shinigamiStates.TryGetValue(creature, out var state) && state.Active;
    }

    public static void SetStoredHp(Creature creature, decimal hp)
    {
        GetOrCreateState(creature).StoredHp = hp;
    }

    public static CardModel? GetOriginalCard(CardModel ofudaCard)
    {
        return _ofudaTransformedCards.GetValueOrDefault(ofudaCard);
    }

    public static void RemoveTransformEntry(CardModel ofudaCard)
    {
        _ofudaTransformedCards.Remove(ofudaCard);
    }

    /// <summary>
    /// Transforms a single card into an Ofuda, storing a backup clone.
    /// Called by ShinigamiPower hooks to catch cards that arrive mid-shinigami-form.
    /// </summary>
    public static async Task TransformToOfuda(CardModel card)
    {
        var combatState = card.CombatState;
        var backup = combatState.CloneCard(card);
        var ofuda = combatState.CreateCard<Ofuda>(card.Owner);
        _ofudaTransformedCards[ofuda] = backup;
        await CardCmd.Transform(card, ofuda);
    }

    public static async Task EnterShinigamiForm(Player player)
    {
        var creature = player.Creature;
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
        if (!_shinigamiStates.TryGetValue(creature, out var state) || !state.Active) return;

        state.Active = false;
        state.ShinigamiCurrentHp = creature.CurrentHp;

        await RestoreAllCards(state.Player);

        creature.SetMaxHpInternal(state.PreShinigamiMaxHp);

        decimal targetHp = state.StoredHp ?? 1m;
        await CreatureCmd.SetCurrentHp(creature, targetHp);

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
            await TransformToOfuda(card);
        }
    }

    private static async Task RestoreAllCards(Player? player)
    {
        if (player == null) return;

        // Collect only the ofuda belonging to this player
        var toRestore = _ofudaTransformedCards
            .Where(kvp => kvp.Key.Owner == player)
            .ToList();

        if (toRestore.Count == 0) return;

        foreach (var (ofuda, backup) in toRestore)
        {
            await CardCmd.Transform(ofuda, backup);
            _ofudaTransformedCards.Remove(ofuda);
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
        _shinigamiStates.Clear();
        _ofudaTransformedCards.Clear();
    }

    public static async Task RitualDeath(Creature creature)
    {
        await BeforeRitualDeath(creature);
        await CreatureCmd.Kill(creature);
    }

    private static async Task BeforeRitualDeath(Creature creature)
    {
        SetStoredHp(creature, creature.CurrentHp);

        foreach (var listener in GetListeners<IWandererEventListener>(creature))
        {
            await listener.BeforeRitualDeath(creature);
        }
    }

    public static async Task EnterShinigami(Player player)
    {
        await EnterShinigamiForm(player);
        await AfterEnteredShinigami(player.Creature);
    }

    private static async Task AfterEnteredShinigami(Creature creature)
    {
        foreach (var listener in GetListeners<IWandererEventListener>(creature))
        {
            await listener.AfterEnteredShinigami(creature);
        }
    }

    private static List<T> GetListeners<T>(Creature creature)
    {
        var listeners = new List<T>();

        var player = creature.Player;
        if (player != null)
        {
            listeners.AddRange(PileType.Hand.GetPile(player).Cards.OfType<T>());
            listeners.AddRange(PileType.Draw.GetPile(player).Cards.OfType<T>());
            listeners.AddRange(PileType.Discard.GetPile(player).Cards.OfType<T>());
            listeners.AddRange(PileType.Exhaust.GetPile(player).Cards.OfType<T>());
        }

        listeners.AddRange(creature.Powers.OfType<T>());

        return listeners;
    }

    public static async Task TransformToRandomFromPool(CardModel card, Player player)
    {
        var options = player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint);
        var transformation = new CardTransformation(card, options);
        await CardCmd.Transform(transformation.Yield(), player.RunState.Rng.CombatCardGeneration);
    }
}
