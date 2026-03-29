using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
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

    public static bool InShinigamiForm { get; private set; } = false;

    /// <summary>
    /// HP to restore when exiting shinigami form.
    /// Set by intentional death cards (Seppuku) before killing.
    /// Null means unintentional death — restore to 1 HP.
    /// </summary>
    public static decimal? StoredHp { get; set; } = null;

    private static int _preShinigamiMaxHp;
    private static Color? _originalModulate;
    private static Player? _player;

    /// <summary>
    /// Maps Ofuda → backup clone of the original card it replaced.
    /// Backups are created via CloneCard before transforming, so they remain
    /// properly registered in CombatState and can be transformed back cleanly.
    /// </summary>
    private static readonly Dictionary<CardModel, CardModel> _transformedCards = new();

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
        if (InShinigamiForm) return;

        _preShinigamiMaxHp = creature.MaxHp;
        _player = player;
        InShinigamiForm = true;

        creature.SetMaxHpInternal(ShinigamiMaxHp);
        await CreatureCmd.Heal(creature, ShinigamiMaxHp);

        await PowerCmd.Apply<ShinigamiPower>(creature, ExhaustThreshold, creature, null);

        await TransformAllCards(player);

        ApplyShinigamiTint(creature);
    }

    public static async Task ExitShinigamiForm(Creature creature)
    {
        if (!InShinigamiForm) return;

        InShinigamiForm = false;

        await RestoreAllCards();

        creature.SetMaxHpInternal(_preShinigamiMaxHp);

        decimal healTo = StoredHp ?? 1m;
        await CreatureCmd.Heal(creature, healTo);

        StoredHp = null;

        await PowerCmd.Remove<ShinigamiPower>(creature);

        ResetShinigamiTint(creature);
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

    private static async Task RestoreAllCards()
    {
        if (_player == null || _transformedCards.Count == 0)
        {
            _transformedCards.Clear();
            return;
        }

        var firstOfuda = _transformedCards.Keys.First();
        var combatState = firstOfuda.CombatState;

        foreach (var (ofuda, backup) in _transformedCards.ToList())
        {
            await CardCmd.Transform(ofuda, backup);
        }
        _transformedCards.Clear();
    }

    private static void ApplyShinigamiTint(Creature creature)
    {
        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (creatureNode == null) return;

        var body = creatureNode.Body;
        _originalModulate ??= body.Modulate;
        body.Modulate = ShinigamiTint;
    }

    private static void ResetShinigamiTint(Creature creature)
    {
        if (_originalModulate == null) return;

        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (creatureNode == null) return;

        creatureNode.Body.Modulate = _originalModulate.Value;
        _originalModulate = null;
    }

    /// <summary>
    /// Call at the start of each combat to reset state.
    /// </summary>
    public static void Reset()
    {
        InShinigamiForm = false;
        StoredHp = null;
        _preShinigamiMaxHp = 0;
        _originalModulate = null;
        _player = null;
        _transformedCards.Clear();
    }
}
