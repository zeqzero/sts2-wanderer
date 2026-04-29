using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>steady, draw</tags>
/// <art>wanderer in partial squat, gedan position with sword pointed down, drawing in a long deep breath, plumes of smoke</art>
/// <kanji>長息</kanji>
[Pool(typeof(WandererCardPool))]
public class LongDraws : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<LongDrawsPower>(1)];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips =>
    [
        HoverTipFactory.FromPower<LongDrawsPower>(),
        HoverTipFactory.FromPower<SteadyPower>(),
    ];

    public LongDraws() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<LongDrawsPower>(Owner.Creature, DynamicVars["LongDrawsPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["LongDrawsPower"].UpgradeValueBy(1);
    }
}
