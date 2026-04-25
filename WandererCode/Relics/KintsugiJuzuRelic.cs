using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Relics;

/// <art>cracked juzu repaired with gold</art>
[Pool(typeof(WandererRelicPool))]
public class KintsugiJuzuRelic : BrokenJuzuRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
}
