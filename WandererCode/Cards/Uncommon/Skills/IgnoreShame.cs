using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>exhaust, counter, redesign</tags>
/// <art></art>
[Pool(typeof(WandererCardPool))]
public class IgnoreShame : WandererCard
{
    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => WasCardExhaustedThisTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(18m, ValueProp.Move)];

    private bool WasCardExhaustedThisTurn => CombatManager.Instance.History.Entries
        .OfType<CardExhaustedEntry>()
        .Any(e => e.HappenedThisTurn(CombatState) && e.Card.Owner == Owner);

    public IgnoreShame() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (WasCardExhaustedThisTurn)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(6m);
    }
}
