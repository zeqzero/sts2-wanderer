using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using Wanderer.WandererCode.Cards;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Relics;

[Pool(typeof(WandererRelicPool))]
public class BrokenJuzuRelic : WandererRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ HoverTipFactory.FromCard<Ofuda>(), HoverTipFactory.FromPower<ShinigamiPower>() ];

    public override async Task BeforeCombatStart()
    {
        ShinigamiCmd.Reset();
    }

    public override bool ShouldDieLate(Creature creature)
    {
        if (creature != Owner.Creature)
            return true;

        if (ShinigamiCmd.InShinigamiForm)
            return true; // In shinigami form, die for real

        return false; // Prevent death, enter shinigami form
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        Flash();
        await ShinigamiCmd.EnterShinigamiForm(creature, Owner);
    }
}
