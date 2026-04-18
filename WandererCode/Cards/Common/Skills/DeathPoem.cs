using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>death, draw, energy, aoe</tags>
/// <art>wanderer writing with an ink brush on parchment, parchment shows a skull, tight zoom on brush and parchment</art>
/// <kanji>辞世</kanji>
[Pool(typeof(WandererCardPool))]
public class DeathPoem : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new EnergyVar(1)];

    public DeathPoem() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Talk();
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    private async Task Talk()
    {
        TalkCmd.Play(LocString.GetRandomWithPrefix("characters", "WANDERER-WANDERER-POEM"), Owner.Creature, vfxColor: VfxColor.White);
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (wasRemovalPrevented || Owner.Creature.IsDead)
            return;

        CardPile? pile = Pile;
        if (pile == null || !pile.IsCombatPile)
            return;

        await CardCmd.AutoPlay(choiceContext, this, null);
    }

    public override async Task BeforeRitualDeath(Creature creature)
    {
        if (creature != Owner.Creature)
            return;

        CardPile? pile = Pile;
        if (pile == null || !pile.IsCombatPile)
            return;

        await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), this, null);
    }
}
