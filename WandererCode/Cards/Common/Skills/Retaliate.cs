using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>counter, aoe</tags>
/// <art>wanderer in gedan, confident posture, perhaps taunting with one hand</art>
/// <kanji>反</kanji>
[Pool(typeof(WandererCardPool))]
public class Retaliate : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ new BlockVar(3, ValueProp.Move), new PowerVar<RetaliatePower>(1) ];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromPower<CounterPower>()];

    public Retaliate() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
        await PowerCmd.Apply<RetaliatePower>(Owner.Creature, DynamicVars["RetaliatePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}
