using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using Wanderer.WandererCode.Cards;
using Wanderer.WandererCode.Interfaces;
using Wanderer.WandererCode.Keywords;
using Wanderer.WandererCode.Nodes;
using Wanderer.WandererCode.Powers;
using MegaCrit.Sts2.Core.Combat;

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
    private static readonly Dictionary<Creature, int> _enteredStanceCounts = new();
    public static int GetEnteredStanceCounts(Creature creature) => _enteredStanceCounts.TryGetValue(creature, out var count) ? count : 0;

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
    /// Backups are created via CloneCard before shifting, so they remain
    /// properly registered in CombatState and can be shifted back cleanly.
    /// </summary>
    private static readonly Dictionary<CardModel, CardModel> _ofudaShiftedCards = new();

    /// <summary>
    /// Cards that were in the Play pile when Shinigami form started.
    /// These couldn't be shifted immediately, so ShinigamiPower.AfterCardChangedPiles
    /// watches for them to leave Play and shifts them then. Entries are consumed on match.
    /// </summary>
    private static readonly HashSet<CardModel> _pendingShinigamiShifts = new();
    public static bool ConsumePendingShinigamiShift(CardModel card) => _pendingShinigamiShifts.Remove(card);

    public static IStancePower? GetCurrentStancePower(Creature creature)
    {
        return creature.Powers.OfType<IStancePower>().FirstOrDefault();
    }

    /// <summary>
    /// Removes the current stance power and applies a new one.
    /// Fires AfterStanceLeft then AfterStanceEntered on all IWandererEventListener cards/powers.
    /// </summary>
    public static async Task EnterStance(Creature creature, Stance stance)
    {
        var oldStancePower = GetCurrentStancePower(creature);

        if (oldStancePower != null)
        {
            await PowerCmd.Remove((PowerModel)oldStancePower);
            await AfterStanceLeft(creature, oldStancePower.Stance);
        }

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

        _enteredStanceCounts[creature] = GetEnteredStanceCounts(creature) + 1;

        WandererVisuals.SetStance(creature, stance.ToString().ToLowerInvariant());

        await AfterStanceEntered(creature, stance);
    }

    private static async Task AfterStanceLeft(Creature creature, Stance oldStance)
    {
        foreach (var listener in GetListeners<IWandererEventListener>(creature))
        {
            await listener.AfterStanceLeft(creature, oldStance);
        }
    }

    private static async Task AfterStanceEntered(Creature creature, Stance stance)
    {
        foreach (var listener in GetListeners<IWandererEventListener>(creature))
        {
            await listener.AfterStanceEntered(creature, stance);
        }
    }

    private static async Task AfterShifted(CardModel card)
    {
        var creature = card.Owner?.Creature;
        if (creature == null) return;

        foreach (var listener in GetListeners<IWandererEventListener>(creature))
        {
            await listener.AfterShifted(card);
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
        return _ofudaShiftedCards.GetValueOrDefault(ofudaCard);
    }

    public static void RemoveShiftEntry(CardModel ofudaCard)
    {
        _ofudaShiftedCards.Remove(ofudaCard);
    }

    /// <summary>
    /// Shifts a single card into an Ofuda, storing a backup clone for later restoration.
    /// Called during EnterShinigamiForm for bulk shifts, and by ShinigamiPower.AfterCardChangedPiles
    /// to catch cards that leave the Play pile after form entry (e.g. Seppuku resolving).
    /// </summary>
    public static async Task ShiftToOfuda(CardModel card)
    {
        if (card.Keywords.Contains(WandererKeywords.Enshrined))
            return;

        var combatState = card.CombatState;
        var backup = combatState.CloneCard(card);
        var ofuda = combatState.CreateCard<Ofuda>(card.Owner);
        _ofudaShiftedCards[ofuda] = backup;
        await CardCmd.Transform(card, ofuda);
        await AfterShifted(card);
    }

    /// <summary>
    /// Entry point for entering shinigami form. Called from BrokenJuzuRelic.AfterPreventingDeath.
    /// Sets max HP to ShinigamiMaxHp, heals the creature, applies ShinigamiPower,
    /// shifts all cards to Ofuda, then fires AfterEnteredShinigami on listeners.
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

        // Cards in Play can't be shifted yet — track them for ShinigamiPower.AfterCardChangedPiles.
        _pendingShinigamiShifts.UnionWith(PileType.Play.GetPile(player).Cards);

        await ShiftAllCards(player);

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

    private static async Task ShiftAllCards(Player player)
    {
        var allCards = new List<CardModel>();
        allCards.AddRange(PileType.Hand.GetPile(player).Cards);
        allCards.AddRange(PileType.Draw.GetPile(player).Cards);
        allCards.AddRange(PileType.Discard.GetPile(player).Cards);

        foreach (var card in allCards)
        {
            await ShiftToOfuda(card);
        }
    }

    private static async Task RestoreAllCards(Player? player)
    {
        if (player == null) return;

        var toRestore = _ofudaShiftedCards
            .Where(kvp => kvp.Key.Owner == player)
            .ToList();

        if (toRestore.Count == 0) return;

        foreach (var (ofuda, backup) in toRestore)
        {
            await CardCmd.Transform(ofuda, backup);
            _ofudaShiftedCards.Remove(ofuda);
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
        _ofudaShiftedCards.Clear();
        _pendingShinigamiShifts.Clear();

        _enteredStanceCounts.Clear();
        JodanEnabled = false;
        WakiEnabled = false;
    }

    /// <summary>
    /// "Shift" a card, transforming it into a random card from the player's card pool.
    /// Uses CombatCardGeneration RNG seed. If upgrade is true, the resulting card is upgraded.
    /// </summary>
    public static async Task ShiftCard(CardModel card, Player player, bool upgrade = false, IEnumerable<CardKeyword>? addKeywords = null)
    {
        if (card.Keywords.Contains(WandererKeywords.Enshrined))
            return;

        CardModel? resultCard;

        // If this is an Ofuda with a tracked original, revert instead of random-shifting.
        var original = GetOriginalCard(card);
        if (original != null)
        {
            await CardCmd.Transform(card, original);
            RemoveShiftEntry(card);
            resultCard = original;
        }
        else
        {
            var options = player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint);
            var transformation = new CardTransformation(card, options);
            var results = await CardCmd.Transform(transformation.Yield(), player.RunState.Rng.CombatCardGeneration);
            resultCard = results.FirstOrDefault().cardAdded;
        }

        if (resultCard != null)
        {
            if (upgrade && resultCard.IsUpgradable)
            {
                CardCmd.Upgrade(resultCard);
            }

            if (addKeywords != null)
            {
                foreach (var keyword in addKeywords)
                {
                    resultCard.AddKeyword(keyword);
                }
            }
        }

        await AfterShifted(card);
    }

    private static readonly LocString ShiftSelectionPrompt = new("card_selection", "WANDERER-TO_SHIFT");

    /// <summary>
    /// Prompts the player to select a card from hand to Shift (excluding Enshrined cards),
    /// then shifts the selected card. If upgrade is true, each resulting shifted card is upgraded.
    /// </summary>
    public static async Task PickAndShiftCardsFromHand(PlayerChoiceContext context, int count, Player player, AbstractModel source, bool upgrade = false, IEnumerable<CardKeyword>? addKeywords = null)
    {
        var prefs = new CardSelectorPrefs(ShiftSelectionPrompt, count);
        var selected = await CardSelectCmd.FromHand(context, player, prefs, c => !c.Keywords.Contains(WandererKeywords.Enshrined), source);

        foreach (var card in selected)
        {
            await ShiftCard(card, player, upgrade, addKeywords);
        }
    }

    /// <summary>
    /// Presents cards on the ChooseACard screen and returns the player's selection.
    /// Equivalent to CardSelectCmd.FromChooseACardScreen but without the 3-card limit.
    /// Handles multiplayer sync, test selectors, and mark-as-seen bookkeeping.
    /// </summary>
    public static async Task<CardModel?> ChooseCard(PlayerChoiceContext context, IReadOnlyList<CardModel> cards, Player player, bool canSkip = false)
    {
        uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
        await context.SignalPlayerChoiceBegun(PlayerChoiceOptions.None);

        CardModel? result;
        if (LocalContext.IsMe(player) && RunManager.Instance.NetService.Type != NetGameType.Replay)
        {
            NPlayerHand.Instance?.CancelAllCardPlay();

            if (CardSelectCmd.Selector != null)
            {
                result = (await CardSelectCmd.Selector.GetSelectedCards(cards, 0, 1)).FirstOrDefault();
            }
            else
            {
                var screen = NChooseACardSelectionScreen.ShowScreen(cards, canSkip);
                if (LocalContext.IsMe(player))
                {
                    foreach (var card in cards)
                    {
                        SaveManager.Instance.MarkCardAsSeen(card);
                    }
                }
                result = (await screen!.CardsSelected()).FirstOrDefault();
            }

            int index = cards.IndexOf(result);
            RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(
                player, choiceId, PlayerChoiceResult.FromIndex(index));
        }
        else
        {
            int remoteIndex = (await RunManager.Instance.PlayerChoiceSynchronizer
                .WaitForRemoteChoice(player, choiceId)).AsIndex();
            result = remoteIndex < 0 ? null : cards[remoteIndex];
        }

        await context.SignalPlayerChoiceEnded();
        return result;
    }

    /// <summary>
    /// Adds a Dishonor curse to the player's deck (permanent) and a combat copy to their hand
    /// for an immediate effect. The deck and hand copies are necessarily distinct CardModels
    /// since the deck lives in RunState and hand cards live in CombatState.
    /// </summary>
    public static async Task AddDishonor(Player player, CombatState? combatState)
    {
        await CardPileCmd.AddCurseToDeck<Dishonor>(player);
        await CardPileCmd.AddToCombatAndPreview<Dishonor>(player.Creature, PileType.Hand, 1, addedByPlayer: true);

        if (combatState != null)
        {
            foreach (var enemy in combatState.HittableEnemies)
            {
                var mostDishonorableString = LocString.GetIfExists("characters", "WANDERER-WANDERER.mostDishonorable");
                if (mostDishonorableString != null)
                    TalkCmd.Play(mostDishonorableString, enemy);
            }
        }
    }
}
