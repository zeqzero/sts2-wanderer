using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Relics;

[Pool(typeof(WandererRelicPool))]
public class UnstrungJuzuRelic : BrokenJuzuRelic
{
    public const int ShinigamiMaxHpBonus = 5;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override async Task AfterObtained()
    {
        WandererCmd.EnsureShinigamiMaxHpBonus(Owner);
    }
}
