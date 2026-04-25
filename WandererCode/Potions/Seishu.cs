using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Potions;

/// <art>a sake bottle of clear sake</art>
[Pool(typeof(WandererPotionPool))]
public class Seishu : WandererPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        AssertValidForTargetedPotion(target);
        var player = target.Player!;

        // Process KimePower first so it can count other next-turn powers before they remove themselves.
        var powers = player.Creature.Powers.ToList();
        foreach (var power in powers.Where(p => p is WandererNextTurnKimePower))
        {
            if (NextTurnPowers.Is(power) && power.Amount != 0)
            {
                await NextTurnPowers.ApplyNow(power, choiceContext, player);
            }
        }
        foreach (var power in powers.Where(p => p is not WandererNextTurnKimePower))
        {
            if (NextTurnPowers.Is(power) && power.Amount != 0)
            {
                await NextTurnPowers.ApplyNow(power, choiceContext, player);
            }
        }
    }
}
