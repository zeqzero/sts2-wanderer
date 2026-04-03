using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>weak, vulnerable</tags>
[Pool(typeof(TokenCardPool))]
public class ShiftGedan : WandererCard, IShiftStance
{
    public override CardPoolModel Pool => ModelDb.CardPool<TokenCardPool>();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<GedanPower>()];

    public ShiftGedan() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, false, false)
    {
    }

    public async Task OnShift(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await StanceCmd.Shift(Owner.Creature, Stance.Gedan);
    }
}