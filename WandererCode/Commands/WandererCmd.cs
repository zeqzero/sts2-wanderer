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
using Wanderer.WandererCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Vfx;

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
/// Central hub for Wanderer mechanics: stances, shinigami form, shift, ritual death.
/// Fires IWandererEventListener hooks on cards/powers. Per-combat state cleared via Reset().
/// </summary>
public static class WandererCmd
{
    // shinigami vars
    public static int DefaultShinigamiMaxHp { get; private set; } = 5;
    public static int ShinigamiExhaustThreshold { get; private set; } = 6;
    private static readonly Color ShinigamiTint = new(Colors.White, 0.3f);

    // stance vars
    private static readonly HashSet<Creature> _jodanEnabled = new();
    private static readonly HashSet<Creature> _wakiEnabled = new();
    public static bool IsJodanEnabled(Creature creature) => _jodanEnabled.Contains(creature);
    public static bool IsWakiEnabled(Creature creature) => _wakiEnabled.Contains(creature);
    public static void EnableJodan(Creature creature) => _jodanEnabled.Add(creature);
    public static void EnableWaki(Creature creature) => _wakiEnabled.Add(creature);
    private static readonly Dictionary<Creature, int> _enteredStanceCounts = new();
    public static int GetEnteredStanceCounts(Creature creature) => _enteredStanceCounts.TryGetValue(creature, out var count) ? count : 0;

    // shift counter: total shifts per combat (incremented inside AfterShifted).
    private static readonly Dictionary<Creature, int> _shiftCounts = new();
    public static int GetShiftCount(Creature creature) => _shiftCounts.TryGetValue(creature, out var count) ? count : 0;

    /// <summary>
    /// Returns true if next-turn powers should NOT remove themselves after activating.
    /// Checked by all IWandererNextTurnPower implementations during cleanup.
    /// </summary>
    public static bool ShouldPreserveNextTurnPowers(Creature creature) => creature.HasPower<JigokuJunbiPower>();
    private static void IncrementShiftCount(Creature creature)
    {
        _shiftCounts[creature] = GetShiftCount(creature) + 1;
    }

    // Transient per-combat Shinigami state.
    private class ShinigamiState
    {
        public bool Active;
        public decimal? StoredHp;
        public int PreShinigamiMaxHp;
        public Color? OriginalModulate;
        public Player? Player;
    }

    private static readonly Dictionary<Creature, ShinigamiState> _shinigamiStates = new();

    // Ofuda → CloneCard backup of the original it replaced, for later revert.
    private static readonly Dictionary<CardModel, CardModel> _ofudaShiftedCards = new();

    // Cards in the Play pile when Shinigami form started; shifted on next pile change.
    private static readonly HashSet<CardModel> _pendingShinigamiShifts = new();
    public static bool ConsumePendingShinigamiShift(CardModel card) => _pendingShinigamiShifts.Remove(card);

    // Random-shift result → backup of the Refill source it came from. On next shift, revert and fire AfterRefilled.
    private static readonly Dictionary<CardModel, CardModel> _refillBackups = new();

    public static IStancePower? GetCurrentStancePower(Creature creature)
    {
        return creature.Powers.OfType<IStancePower>().FirstOrDefault();
    }

    public static Stance? GetRandomStance(Creature creature, bool different)
    {
        List<Stance> candidates = [Stance.Chudan, Stance.Hasso, Stance.Gedan];

        if (IsJodanEnabled(creature))
            candidates.Add(Stance.Jodan);
        if (IsWakiEnabled(creature))
            candidates.Add(Stance.Waki);

        var currentStance = GetCurrentStancePower(creature);
        if (different && currentStance != null && candidates.Contains(currentStance.Stance))
        {
            candidates.Remove(currentStance.Stance);
        }

        return creature.Player.RunState.Rng.CombatCardSelection.NextItem(candidates);
    }

    /// <summary>Swap stance power, firing AfterStanceLeft/AfterStanceEntered on listeners.</summary>
    public static async Task EnterStance(Creature creature, Stance stance, int amount)
    {
        var oldStancePower = GetCurrentStancePower(creature);

        // if we're entering a new stance, leave the old one
        if (oldStancePower != null && oldStancePower.Stance != stance)
        {
            // if we have Fudoshin, cancel leaving and entering
            if (creature.Powers.OfType<FudoshinPower>().Any())
                return;

            await PowerCmd.Remove((PowerModel)oldStancePower);
            await AfterStanceLeft(creature, oldStancePower.Stance);
        }

        switch (stance)
        {
            case Stance.Chudan:
                await PowerCmd.Apply<ChudanPower>(creature, amount, creature, null);
                break;
            case Stance.Hasso:
                await PowerCmd.Apply<HassoPower>(creature, amount, creature, null);
                break;
            case Stance.Gedan:
                await PowerCmd.Apply<GedanPower>(creature, amount, creature, null);
                break;
            case Stance.Jodan:
                await PowerCmd.Apply<JodanPower>(creature, amount, creature, null);
                EnableJodan(creature);
                break;
            case Stance.Waki:
                await PowerCmd.Apply<WakiPower>(creature, amount, creature, null);
                EnableWaki(creature);
                break;
        }

        _enteredStanceCounts[creature] = GetEnteredStanceCounts(creature) + amount;

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

        IncrementShiftCount(creature);

        foreach (var listener in GetListeners<IWandererEventListener>(creature))
        {
            await listener.AfterShifted(card);
        }
    }

    public static bool InShinigamiForm(Creature creature)
    {
        return _shinigamiStates.TryGetValue(creature, out var state) && state.Active;
    }

    private static BrokenJuzuRelic? GetJuzuRelic(Creature creature)
    {
        return creature.Player?.Relics.OfType<BrokenJuzuRelic>().FirstOrDefault();
    }

    public static bool IsShinigamiHpBelowMax(Creature creature)
    {
        var relic = GetJuzuRelic(creature);
        return relic != null && relic.ShinigamiCurrentHp < relic.ShinigamiMaxHp;
    }

    public static int GetShinigamiCurrentHp(Creature creature)
    {
        if (InShinigamiForm(creature))
            return creature.CurrentHp;
        return GetJuzuRelic(creature)?.ShinigamiCurrentHp ?? DefaultShinigamiMaxHp;
    }

    public static int GetShinigamiMaxHp(Creature creature)
    {
        if (InShinigamiForm(creature))
            return creature.MaxHp;
        return GetJuzuRelic(creature)?.ShinigamiMaxHp ?? DefaultShinigamiMaxHp;
    }

    private static int GetShinigamiMaxHpBonus(Player player)
    {
        return player.Relics.OfType<UnstrungJuzuRelic>().Any() ? UnstrungJuzuRelic.ShinigamiMaxHpBonus : 0;
    }

    /// <summary>
    /// Raises the persistent Shinigami max HP pool to its target for the player's current
    /// relics, healing current HP by the same delta. Called from UnstrungJuzuRelic.AfterObtained.
    /// </summary>
    public static void EnsureShinigamiMaxHpBonus(Player player)
    {
        var relic = GetJuzuRelic(player.Creature);
        if (relic == null) return;

        int targetMaxHp = DefaultShinigamiMaxHp + GetShinigamiMaxHpBonus(player);
        if (relic.ShinigamiMaxHp >= targetMaxHp) return;

        int delta = targetMaxHp - relic.ShinigamiMaxHp;
        relic.ShinigamiMaxHp = targetMaxHp;
        relic.ShinigamiCurrentHp += delta;
    }

    /// <summary>Restores the persisted shinigami HP pool to max (out-of-combat healing).</summary>
    public static void FullyHealShinigami(Creature creature)
    {
        var relic = GetJuzuRelic(creature);
        if (relic != null)
        {
            relic.ShinigamiCurrentHp = relic.ShinigamiMaxHp;
        }
    }

    private static void SetStoredHp(Creature creature, decimal hp)
    {
        GetOrCreateState(creature).StoredHp = hp;
    }

    private static CardModel? GetOriginalCard(CardModel ofudaCard)
    {
        return _ofudaShiftedCards.GetValueOrDefault(ofudaCard);
    }

    /// <summary>
    /// Revert an Ofuda back to its tracked original, firing AfterShifted and (if applicable)
    /// AfterRefilled on the restored card. Shared by RestoreAllCards and HandleOfudaExhausted.
    /// </summary>
    private static async Task RevertOfuda(CardModel ofuda, CardModel backup)
    {
        await CardCmd.Transform(ofuda, backup);
        _ofudaShiftedCards.Remove(ofuda);
        await AfterShifted(backup);

        if (backup.Keywords.Contains(WandererKeywords.Refills))
        {
            await AfterRefilled(backup);
        }
    }

    /// <summary>
    /// Called from ShinigamiPower when a card is exhausted. If it's an Ofuda, revert it to
    /// its original and, if that original was a Dishonor, remove one Dishonor from the deck.
    /// </summary>
    public static async Task HandleOfudaExhausted(CardModel card)
    {
        if (card is not Ofuda) return;

        var backup = GetOriginalCard(card);
        if (backup == null) return;

        await RevertOfuda(card, backup);

        if (backup is Dishonor && card.Owner != null)
        {
            var cardToRemove = card.Owner.Deck.Cards.FirstOrDefault(c => c is Dishonor);
            if (cardToRemove != null)
            {
                await CardPileCmd.RemoveFromDeck(cardToRemove, true);
            }
        }
    }

    /// <summary>
    /// Shifts a single card into an Ofuda with a backup for later revert. Used by
    /// EnterShinigamiForm bulk shifts and ShinigamiPower for cards entering Play mid-combat.
    /// </summary>
    public static async Task ShiftToOfuda(CardModel card)
    {
        if (card.Keywords.Contains(WandererKeywords.Enshrined))
            return;

        var combatState = card.CombatState;
        CardModel backup;
        // If this card is a pending Refill revert, forward the Refill original as the Ofuda
        // backup so a later Ofuda revert restores the Refill source and fires AfterRefilled.
        if (_refillBackups.TryGetValue(card, out var existingRefillBackup))
        {
            backup = existingRefillBackup;
            _refillBackups.Remove(card);
        }
        else
        {
            backup = combatState.CloneCard(card);
        }
        var ofuda = combatState.CreateCard<Ofuda>(card.Owner);
        _ofudaShiftedCards[ofuda] = backup;
        await CardCmd.Transform(card, ofuda);
        await AfterShifted(card);
    }

    /// <summary>Entry point from BrokenJuzuRelic.AfterPreventingDeath.</summary>
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

        var relic = GetJuzuRelic(creature);
        if (relic == null) return;

        // Covers saves made before Unstrung Juzu's bonus was applied (e.g. mod upgrade).
        EnsureShinigamiMaxHpBonus(player);

        // HP-to-0 entry pays 1 Shinigami HP so the transition isn't a freebie soaked hit.
        // RitualDeath sets StoredHp before Kill, so StoredHp==null identifies the HP-to-0 path.
        if (state.StoredHp == null && relic.ShinigamiCurrentHp > 0)
        {
            relic.ShinigamiCurrentHp--;
        }

        state.PreShinigamiMaxHp = creature.MaxHp;
        state.Player = player;
        state.Active = true;

        creature.SetMaxHpInternal(relic.ShinigamiMaxHp);
        await CreatureCmd.Heal(creature, relic.ShinigamiCurrentHp);

        await PowerCmd.Apply<ShinigamiPower>(creature, ShinigamiExhaustThreshold, creature, null);

        // Cards mid-resolution in Play can't be shifted now; ShinigamiPower picks them up on pile change.
        _pendingShinigamiShifts.UnionWith(PileType.Play.GetPile(player).Cards);

        await ShiftAllCardsToOfuda(player);

        ApplyShinigamiTint(creature, state);
    }

    /// <summary>
    /// Called from ShinigamiPower when the exhaust counter hits zero. Reverts Ofudas,
    /// restores max HP, sets current HP to the pre-ritual-death value (or 1).
    /// </summary>
    public static async Task ExitShinigamiForm(Creature creature)
    {
        if (!_shinigamiStates.TryGetValue(creature, out var state) || !state.Active) return;

        state.Active = false;
        // Persist any in-form max-hp changes onto the Shinigami HP pool.
        var relic = GetJuzuRelic(creature);
        if (relic != null)
        {
            relic.ShinigamiMaxHp = creature.MaxHp;
            relic.ShinigamiCurrentHp = creature.CurrentHp;
        }

        await RestoreAllCards(state.Player);

        creature.SetMaxHpInternal(state.PreShinigamiMaxHp);

        decimal targetHp = state.StoredHp ?? 1m;
        await CreatureCmd.SetCurrentHp(creature, targetHp);

        state.StoredHp = null;

        await PowerCmd.Remove<ShinigamiPower>(creature);

        ResetShinigamiTint(creature, state);
    }

    private static async Task ShiftAllCardsToOfuda(Player player)
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

        foreach (var (ofuda, backup) in toRestore)
        {
            // Post-combat Ofudas may be detached from any pile; Transform would throw.
            if (ofuda.Pile != null)
            {
                await RevertOfuda(ofuda, backup);
            }
            else
            {
                _ofudaShiftedCards.Remove(ofuda);
            }
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
    /// Ritual self-kill (e.g. Seppuku). Stores HP, fires BeforeRitualDeath so cards like
    /// DeathPoem can auto-play while alive, then kills. BrokenJuzuRelic enters shinigami form.
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
    /// Snapshot of T-listeners across Hand/Draw/Discard/Exhaust piles and powers.
    /// Play pile is excluded (mid-resolution). Snapshot lets callers iterate safely while piles change.
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
        listeners.AddRange(creature.Player.Relics.OfType<T>());

        return listeners;
    }

    /// <summary>Called from BrokenJuzuRelic.BeforeCombatStart to clear per-combat static state.</summary>
    public static void Reset()
    {
        _ofudaShiftedCards.Clear();
        _pendingShinigamiShifts.Clear();
        _refillBackups.Clear();
        _shiftCounts.Clear();

        _enteredStanceCounts.Clear();
        _jodanEnabled.Clear();
        _wakiEnabled.Clear();
    }

    /// <summary>
    /// Transform a card into a random card from the player's pool (or revert it, if it's
    /// an Ofuda or a Refill result). Fires AfterShifted and, on refill revert, AfterRefilled.
    /// </summary>
    public static async Task ShiftCard(CardModel card, Player player, bool upgrade = false, IEnumerable<CardKeyword>? addKeywords = null)
    {
        if (card.Keywords.Contains(WandererKeywords.Enshrined))
            return;

        CardModel? resultCard;
        bool isRevertShift = false;

        // Ofuda revert path.
        var original = GetOriginalCard(card);
        if (original != null)
        {
            await CardCmd.Transform(card, original);
            _ofudaShiftedCards.Remove(card);
            resultCard = original;
            isRevertShift = true;
        }
        // Refill revert path: second shift of a card produced from a Refill source.
        else if (_refillBackups.TryGetValue(card, out var refillBackup))
        {
            await CardCmd.Transform(card, refillBackup);
            _refillBackups.Remove(card);
            refillBackup.RemoveKeyword(WandererKeywords.Refilling);
            if (refillBackup is WandererCard wandererRefill)
            {
                wandererRefill.ClearRuntimeHoverTips();
            }
            resultCard = refillBackup;
            isRevertShift = true;
        }
        else
        {
            // Clone the Refill source before Transform detaches it from its pile.
            CardModel? pendingRefillBackup = null;
            if (card.Keywords.Contains(WandererKeywords.Refills))
            {
                pendingRefillBackup = card.CombatState.CloneCard(card);
            }

            var options = player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint);
            // Landing on Enshrined or Refills would trap the refill chain permanently.
            if (pendingRefillBackup != null)
            {
                options = options.Where(c => !c.Keywords.Contains(WandererKeywords.Enshrined) && !c.Keywords.Contains(WandererKeywords.Refills));
            }
            var transformation = new CardTransformation(card, options);
            var results = await CardCmd.Transform(transformation.Yield(), player.RunState.Rng.CombatCardGeneration);
            resultCard = results.FirstOrDefault().cardAdded;

            if (pendingRefillBackup != null && resultCard != null)
            {
                _refillBackups[resultCard] = pendingRefillBackup;
                resultCard.AddKeyword(WandererKeywords.Refilling);
                if (resultCard is WandererCard wandererResult)
                {
                    wandererResult.AddRuntimeHoverTip(HoverTipFactory.FromCard(pendingRefillBackup));
                }
            }
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

        if (isRevertShift && resultCard != null && resultCard.Keywords.Contains(WandererKeywords.Refills))
        {
            await AfterRefilled(resultCard);
        }
    }

    /// <summary>Fires AfterRefilled on listeners. Public for ShinigamiPower's exhaust-revert path.</summary>
    public static async Task AfterRefilled(CardModel card)
    {
        var creature = card.Owner?.Creature;
        if (creature == null) return;

        foreach (var listener in GetListeners<IWandererEventListener>(creature))
        {
            await listener.AfterRefilled(card);
        }
    }

    /// <summary>Find Kamae or Mu-gamae in draw/discard/exhaust and put it in hand. Prioritizes Mu-gamae.</summary>
    public static async Task PutKamaeInHand(Player player)
    {
        if (PileType.Hand.GetPile(player).Cards.Any(c => c is MuGamae or Kamae))
            return;

        static CardModel? FindInPile(Player p, PileType pileType) =>
            pileType.GetPile(p).Cards.FirstOrDefault(c => c is MuGamae)
            ?? pileType.GetPile(p).Cards.FirstOrDefault(c => c is Kamae);

        CardModel? card = FindInPile(player, PileType.Draw) ?? FindInPile(player, PileType.Discard) ?? FindInPile(player, PileType.Exhaust);
        if (card != null)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    /// <summary>Prompt the player to pick cards from hand (non-Enshrined) and Shift each.</summary>
    private static readonly LocString DefaultShiftPrompt = new("card_selection", "WANDERER-TO_SHIFT");

    public static async Task PickAndShiftCardsFromHand(PlayerChoiceContext context, int count, Player player, AbstractModel source, bool upgrade = false, IEnumerable<CardKeyword>? addKeywords = null, LocString? prompt = null)
    {
        var prefs = new CardSelectorPrefs(prompt ?? DefaultShiftPrompt, 0, count);
        var selected = await CardSelectCmd.FromHand(context, player, prefs, c => !c.Keywords.Contains(WandererKeywords.Enshrined), source);

        foreach (var card in selected)
        {
            await ShiftCard(card, player, upgrade, addKeywords);
        }
    }

    public static async Task PickAndShiftCardsFromPile(PlayerChoiceContext context, CardPile pile, int min, int max, Player player, bool upgrade = false, IEnumerable<CardKeyword>? addKeywords = null, LocString? prompt = null)
    {
        var prefs = new CardSelectorPrefs(prompt ?? DefaultShiftPrompt, min, max);
        var cards = pile.Cards.Where(c => !c.Keywords.Contains(WandererKeywords.Enshrined)).ToList();
        var selected = await CardSelectCmd.FromSimpleGrid(context, cards, player, prefs);

        foreach (var card in selected)
        {
            await ShiftCard(card, player, upgrade, addKeywords);
        }
    }

    /// <summary>
    /// ChooseACard screen selection. Like CardSelectCmd.FromChooseACardScreen but with no 3-card limit.
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
    /// Adds a permanent Dishonor to the deck and a separate combat copy to hand
    /// (deck lives in RunState, hand in CombatState, so they must be distinct CardModels).
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
                    TalkCmd.Play(mostDishonorableString, enemy, vfxColor: VfxColor.White);
            }
        }
    }
}
