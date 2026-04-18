using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>aoe, weak</tags>
/// <art>Enemy with targeting reticles shown on head, arms, and legs, arms reticle is highlighted, retro hand icon (ala old final fantasy) pointing at arms reticle as if it's a menu combat gui</art>
/// <kanji>腕</kanji>
[Pool(typeof(WandererCardPool))]
public class TargetArms : WandererCard
{
    protected override IEnumerable<IHoverTip> WandererExtraHoverTips =>
    [
        HoverTipFactory.FromPower<TargetArmsPower>(),
        HoverTipFactory.FromPower<WeakPower>()
    ];

    public TargetArms() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<TargetArmsPower>(enemy, 1, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
