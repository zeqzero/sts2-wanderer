using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Keywords;

namespace Wanderer.WandererCode.Relics;

/// <art>sake jar on pedestal</art>
[Pool(typeof(WandererRelicPool))]
public class OmikiRelic : WandererRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(WandererKeywords.Refill),
        WandererKeywords.ShiftHoverTip
    ];
}
