using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Cards;

/// <tags>death</tags>
/// <art></art>
[Pool(typeof(WandererCardPool))]
public class Junshi : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];

    public Junshi() : base(4, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await WandererCmd.RitualDeath(Owner.Creature);
    }

    public override async Task BeforeRitualDeath(Creature creature)
    {
        if (Pile == null || !Pile.IsCombatPile)
            return;

        EnergyCost.AddThisCombat(-DynamicVars.Energy.IntValue);
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (Pile == null || !Pile.IsCombatPile)
            return;

        EnergyCost.AddThisCombat(-DynamicVars.Energy.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}