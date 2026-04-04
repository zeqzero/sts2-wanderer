using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Wanderer.WandererCode.Cards;
using Wanderer.WandererCode.Interfaces;
using Wanderer.WandererCode.Nodes;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Commands;

public enum Stance
{
    Chudan,
    Hasso,
    Gedan,
    Jodan,
    Waki
}

/// <summary>
/// Central command hub for Wanderer-specific mechanics: stances, shinigami form, and ritual death.
/// Fires custom lifecycle hooks via IWandererEventListener, discovered dynamically on cards and powers.
/// All static state is reset per-combat via Reset(), called from BrokenJuzuRelic.BeforeCombatStart.
/// </summary>
public static class WandererCmd
{
    // shinigami vars
    public static int ShinigamiMaxHp { get; private set; } = 5;
    public static int ShinigamiExhaustThreshold { get; private set; } = 5;
    private static readonly Color ShinigamiTint = new(Colors.White, 0.3f);

    // stance vars
    public static bool JodanEnabled = false;
    public static bool WakiEnabled = false;
    private static readonly Dictionary<Creature, int> _shiftCounts = new();
    public static int GetShiftCount(Creature creature) => _shiftCounts.TryGetValue(creature, out var count) ? count : 0;

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
    /// </summary>
    private static readonly Dictionary<CardModel, CardModel> _ofudaTransformedCards = new();

    /// <summary>
    /// Removes the current stance power and applies a new one.
    /// Fires AfterShifted on all IWandererEventListener cards/powers after the new stance is active.
    /// </summary>
    public static async Task Shift(Creature creature, Stance stance)
    {
        await PowerCmd.Remove<ChudanPower>(creature);
        await PowerCmd.Remove<HassoPower>(creature);
        await PowerCmd.Remove<GedanPower>(creature);
        await PowerCmd.Remove<JodanPower>(creature);
        await PowerCmd.Remove<WakiPower>(creature);

        switch (stance)
        {
            case Stance.Chudan:
                await PowerCmd.Apply<ChudanPower>(creature, 1, creature, null);
                break;
            case Stance.Hasso:
                await PowerCmd.Apply<HassoPower>(creature, 1, creature, null);
                break;
            case Stance.Gedan:
                await PowerCmd.Apply<GedanPower>(creature, 1, creature, null);
                break;
            case Stance.Jodan:
                await PowerCmd.Apply<JodanPower>(creature, 1, creature, null);
                JodanEnabled = true;
                break;
            case Stance.Waki:
                await PowerCmd.Apply<WakiPower>(creature, 1, creature, null);
                WakiEnabled = true;
                break;
        }

        _shiftCounts[creature] = GetShiftCount(creature) + 1;

        WandererVisuals.SetStance(creature, stance.ToString().ToLowerInvariant());

        await AfterShifted(creature, stance);
    }

    private static async Task AfterShifted(Creature creature, Stance stance)
    {
        foreach (var listener in GetListeners<IWandererEventListener>(creature))
        {
            await listener.AfterShifted(creature, stance);
        }
    }

    public static bool InShinigamiForm(Creature creature)
    {
        return _shinigamiStates.TryGetValue(creature, out var state) && state.Active;
    }

    /// <summary>
    /// Saves the creature's current HP so it can be restored on exit.
    /// </summary>
    private static void SetStoredHp(Creature creature, decimal hp)
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
    /// Transforms a single card into an Ofuda, storing a backup clone for later restoration.
    /// Called during EnterShinigamiForm for bulk transforms, and by ShinigamiPower.AfterCardChangedPiles
    /// to catch cards that leave the Play pile after form entry (e.g. Seppuku resolving).
    /// </summary>
    public static async Task TransformToOfuda(CardModel card)
    {
        var combatState = card.CombatState;
        var backup = combatState.CloneCard(card);
        var ofuda = combatState.CreateCard<Ofuda>(card.Owner);
        _ofudaTransformedCards[ofuda] = backup;
        await CardCmd.Transform(card, ofuda);
    }

    /// <summary>
    /// Entry point for entering shinigami form. Called from BrokenJuzuRelic.AfterPreventingDeath.
    /// Sets max HP to ShinigamiMaxHp, heals the creature, applies ShinigamiPower,
    /// transforms all cards to Ofuda, then fires AfterEnteredShinigami on listeners.
    /// </summary>
    public static async Task EnterShinigami(Player player)
    {
        await EnterShinigamiForm(player);
        await AfterEnteredShinigami(player.Creature);
    }

    private static async Task EnterShinigamiForm(Player player)
    {
        var creature = player.Creature;
        var state = GetOrCreateState(creature);
        if (state.Active) return;

        state.PreShinigamiMaxHp = creature.MaxHp;
        state.Player = player;
        state.Active = true;

        creature.SetMaxHpInternal(ShinigamiMaxHp);
        await CreatureCmd.Heal(creature, state.ShinigamiCurrentHp);

        await PowerCmd.Apply<ShinigamiPower>(creature, ShinigamiExhaustThreshold, creature, null);

        await TransformAllCards(player);

        ApplyShinigamiTint(creature, state);
    }

    /// <summary>
    /// Exits shinigami form. Called from ShinigamiPower when the exhaust counter reaches zero.
    /// Restores all Ofuda back to original cards, resets max HP, sets current HP to the
    /// value stored before ritual death (or 1 if entered via normal damage), and removes ShinigamiPower.
    /// </summary>
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

    private static ShinigamiState GetOrCreateState(Creature creature)
    {
        if (!_shinigamiStates.TryGetValue(creature, out var state))
        {
            state = new ShinigamiState();
            _shinigamiStates[creature] = state;
        }
        return state;
    }

    /// <summary>
    /// Performs a ritual self-kill (e.g. Seppuku). Stores current HP for shinigami restoration,
    /// fires BeforeRitualDeath on listeners (allowing cards like DeathPoem to auto-play while
    /// the creature is still alive), then kills the creature. BrokenJuzuRelic intercepts the
    /// death via ShouldDieLate and enters shinigami form.
    /// </summary>
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

    private static async Task AfterEnteredShinigami(Creature creature)
    {
        foreach (var listener in GetListeners<IWandererEventListener>(creature))
        {
            await listener.AfterEnteredShinigami(creature);
        }
    }

    /// <summary>
    /// Discovers all IWandererEventListener implementations on the creature's cards (in Hand, Draw,
    /// Discard, Exhaust piles) and powers. Returns a snapshot list so callers can safely iterate
    /// while the underlying collections change (e.g. cards moving piles during auto-play).
    /// Note: cards in the Play pile are not included — they are mid-resolution.
    /// </summary>
    public static List<T> GetListeners<T>(Creature creature)
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

    /// <summary>
    /// Must be called at the start of each combat to clear all static state.
    /// Currently called from BrokenJuzuRelic.BeforeCombatStart.
    /// </summary>
    public static void Reset()
    {
        _shinigamiStates.Clear();
        _ofudaTransformedCards.Clear();

        _shiftCounts.Clear();
        JodanEnabled = false;
        WakiEnabled = false;
    }

    /// <summary>
    /// Transforms a card into a random card from the player's card pool.
    /// Uses CombatCardGeneration RNG seed.
    /// </summary>
    public static async Task TransformToRandomFromPool(CardModel card, Player player)
    {
        var options = player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint);
        var transformation = new CardTransformation(card, options);
        await CardCmd.Transform(transformation.Yield(), player.RunState.Rng.CombatCardGeneration);
    }
}
