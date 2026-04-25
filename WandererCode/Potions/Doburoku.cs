using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Potions;

/// <art>a clay jug of cloudy unrefined sake</art>
[Pool(typeof(WandererPotionPool))]
public class Doburoku : WandererPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyPlayer;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [WandererKeywords.ShiftHoverTip];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        AssertValidForTargetedPotion(target);
        var player = target.Player!;
        await WandererCmd.PickAndShiftCardsFromHand(choiceContext, 3, player, this);
    }
}
