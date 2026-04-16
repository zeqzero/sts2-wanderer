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

/// <tags>counter</tags>
/// <art>wanderer holding one-handed tora sign (first with index and middle finger raised) in front of face, zoomed in on hand</art>
/// <kanji>残心</kanji>
[Pool(typeof(WandererCardPool))]
public class Zanshin : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ new BlockVar(24, ValueProp.Move), new PowerVar<CounterPower>(1) ];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [ HoverTipFactory.FromPower<CounterPower>() ];

    public Zanshin() : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<CounterPower>(Owner.Creature, DynamicVars["CounterPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(6);
    }
}