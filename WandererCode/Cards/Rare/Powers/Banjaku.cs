using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>counter</tags>
[Pool(typeof(WandererCardPool))]
public class Banjaku : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PlatingPower>(10m)];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

    public Banjaku() : base(4, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PlatingPower>(Owner.Creature, DynamicVars["PlatingPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PlatingPower"].UpgradeValueBy(2m);
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this || IsClone)
            return;

        int blockGains = CombatManager.Instance.History.Entries
            .OfType<BlockGainedEntry>()
            .Count(e => e.Receiver == Owner.Creature && e.HappenedThisTurn(CombatState));
        EnergyCost.AddThisTurn(-blockGains);
    }

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature != Owner.Creature)
            return;

        EnergyCost.AddThisTurn(-1);
    }
}
