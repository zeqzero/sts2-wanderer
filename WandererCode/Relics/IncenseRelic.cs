using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Relics;

/// <art>incense poking out of ash</art>
[Pool(typeof(WandererRelicPool))]
public class IncenseRelic : WandererRelic
{
    private const int BlockAmount = 8;

    private bool _usedThisCombat;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust), HoverTipFactory.Static(StaticHoverTip.Block)];

    private bool UsedThisCombat
    {
        get => _usedThisCombat;
        set
        {
            AssertMutable();
            _usedThisCombat = value;
            Status = value ? RelicStatus.Disabled : RelicStatus.Active;
        }
    }

    public override async Task BeforeCombatStart()
    {
        UsedThisCombat = false;
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (UsedThisCombat) return;
        if (card.Owner != Owner) return;

        UsedThisCombat = true;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, BlockAmount, ValueProp.Unpowered, null);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        UsedThisCombat = false;
    }
}
