using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Runs;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Relics;

[Pool(typeof(WandererRelicPool))]
public class HachimakiRelic : WandererRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    // Free Misogi: the player may choose Misogi PLUS one other rest-site option.
    // - First pick is Misogi → leave the other options on offer (return false).
    // - First pick is non-Misogi (with Misogi available) → reorder the options list
    //   so the synchronizer's subsequent RemoveAt(optionIndex) leaves only Misogi.
    // - Otherwise → disable as normal.
    public override bool ShouldDisableRemainingRestSiteOptions(Player player)
    {
        if (player != Owner) return true;

        var entry = player.RunState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId);
        if (entry == null) return true;

        var choices = entry.RestSiteChoices;
        if (choices.Count >= 2) return true;

        if (choices.Contains(MisogiRestSiteOption.Id))
        {
            Flash();
            return false;
        }

        var optionsList = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer.GetOptionsForPlayer(player);
        var misogi = optionsList.OfType<MisogiRestSiteOption>().FirstOrDefault();
        if (misogi == null) return true;

        var lastChosenId = choices[choices.Count - 1];
        var chosen = optionsList.FirstOrDefault(o => o.OptionId == lastChosenId);
        if (chosen == null) return true;

        int chosenIdx = optionsList.IndexOf(chosen);

        // The synchronizer will call options.RemoveAt(chosenIdx) after this hook returns false.
        // Reorder so only Misogi survives that RemoveAt. Works cleanly when chosenIdx ∈ {0, 1};
        // higher indices (multiplayer Mend) fall through to a normal disable.
        if (chosenIdx == 0)
        {
            optionsList.Clear();
            optionsList.Add(chosen);
            optionsList.Add(misogi);
            Flash();
            return false;
        }
        if (chosenIdx == 1)
        {
            optionsList.Clear();
            optionsList.Add(misogi);
            optionsList.Add(chosen);
            Flash();
            return false;
        }

        return true;
    }
}
