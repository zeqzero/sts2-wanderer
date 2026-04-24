using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Relics;

[Pool(typeof(WandererRelicPool))]
public class KintsugiJuzuRelic : BrokenJuzuRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
}
