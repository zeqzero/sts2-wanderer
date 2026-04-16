using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>counter</tags>
/// <art>kendo vitruvian man, only gedan visible</art>
/// <kanji>下段</kanji>
[Pool(typeof(TokenCardPool))]
public class EnterGedan : WandererCard, IEnterStance
{
    public override CardPoolModel Pool => ModelDb.CardPool<TokenCardPool>();

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [ HoverTipFactory.FromPower<GedanPower>(), HoverTipFactory.FromPower<CounterPower>() ];

    public EnterGedan() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, false, false)
    {
    }

    public async Task OnEnter(PlayerChoiceContext choiceContext, CardPlay cardPlay, int amount)
    {
        await WandererCmd.EnterStance(Owner.Creature, Stance.Gedan, amount);
    }
}