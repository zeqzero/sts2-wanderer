using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Potions;

/// <art>a chipped cup of murky sake lees</art>
[Pool(typeof(WandererPotionPool))]
public class Kasutori : WandererPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await WandererCmd.RitualDeath(Owner.Creature);
    }
}
