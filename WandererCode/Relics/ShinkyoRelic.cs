using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Relics;

[Pool(typeof(WandererRelicPool))]
public class ShinkyoRelic : WandererRelic
{
    public const int MaxHpIncrease = 5;

    public override RelicRarity Rarity => RelicRarity.Shop;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [BrokenJuzuRelic.ShinigamiPowerCanonicalHoverTip];

    public override async Task AfterObtained()
    {
        var juzu = Owner?.Relics.OfType<BrokenJuzuRelic>().FirstOrDefault();
        if (juzu == null) return;

        juzu.ShinigamiMaxHp += MaxHpIncrease;
        juzu.ShinigamiCurrentHp += MaxHpIncrease;
    }

    public override async Task AfterRemoved()
    {
        var juzu = Owner?.Relics.OfType<BrokenJuzuRelic>().FirstOrDefault();
        if (juzu == null) return;

        juzu.ShinigamiMaxHp -= MaxHpIncrease;
        juzu.ShinigamiCurrentHp = Math.Min(juzu.ShinigamiCurrentHp, juzu.ShinigamiMaxHp);
    }
}
