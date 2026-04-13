using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Powers;

/// <summary>
/// While in shinigami form: all cards cost 0 energy and exhaust when played.
/// Intangible (all damage reduced to 1). After exhausting enough cards, exit shinigami form.
/// </summary>
/// <art></art>
public class ShinigamiPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // --- Intangible: cap all damage to 1 ---
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!CombatManager.Instance.IsInProgress || target != Owner)
        {
            return amount;
        }

        return Math.Min(1m, amount);
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        Flash();
    }

    public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner)
        {
            return decimal.MaxValue;
        }

        return 1m;
    }

    public override async Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
    }

    // --- Track exhausts and exit shinigami form ---
    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card.Owner != Owner.Player) return;

        await WandererCmd.HandleOfudaExhausted(card);

        Amount--;
        if (Amount <= 0)
        {
            await WandererCmd.ExitShinigamiForm(Owner);
        }
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        // Catch cards that were mid-resolution (in Play pile) when Shinigami form started.
        // e.g. Seppuku triggers Shinigami, then lands in discard after transforms already happened.
        if (oldPileType == PileType.Play && WandererCmd.ConsumePendingShinigamiShift(card))
        {
            await WandererCmd.ShiftToOfuda(card);
        }
    }

    public override async Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
    {
        if (card.Owner.Creature == Owner && !addedByPlayer)
        {
            await WandererCmd.ShiftToOfuda(card);
        }
    }
}
