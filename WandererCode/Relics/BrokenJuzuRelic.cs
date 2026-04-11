using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Wanderer.WandererCode.Cards;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Relics;

[Pool(typeof(WandererRelicPool))]
public class BrokenJuzuRelic : WandererRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public static HoverTip ShinigamiPowerCanonicalHoverTip
    {
        get
        {
            var desc = new LocString("powers", "WANDERER-SHINIGAMI_POWER.description");
            desc.Add("Amount", WandererCmd.ShinigamiExhaustThreshold);
            return new HoverTip(ModelDb.Power<ShinigamiPower>(), desc.GetFormattedText(), false);
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Ofuda>(), ShinigamiPowerCanonicalHoverTip];

    public override async Task BeforeCombatStart()
    {
        WandererCmd.Reset();
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (WandererCmd.InShinigamiForm(Owner.Creature))
        {
            await WandererCmd.ExitShinigamiForm(Owner.Creature);
        }
    }

    public override bool ShouldDie(Creature creature)
    {
        if (creature != Owner.Creature)
            return true;

        if (WandererCmd.InShinigamiForm(creature))
            return true; // In shinigami form, die for real

        return false; // Prevent death, enter shinigami form
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        Flash();
        await WandererCmd.EnterShinigami(Owner);
    }

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner)
        {
            return false;
        }

        bool hasDishonor = player.Deck.Cards.Any(c => c is Dishonor);
        bool needsShinigamiHeal = WandererCmd.IsShinigamiHpBelowMax(player.Creature);

        if (!hasDishonor && !needsShinigamiHeal)
            return false;

        options.Add(new MisogiRestSiteOption(player));
        return true;
    }

}
