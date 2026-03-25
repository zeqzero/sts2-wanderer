using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

[Pool(typeof(TokenCardPool))]
public class ShiftHasso : CustomCardModel, IShiftStance
{
    public override CardPoolModel Pool => ModelDb.CardPool<TokenCardPool>();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ HoverTipFactory.FromPower<HassoPower>() ];

    public ShiftHasso() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, false, false)
    {
    }

    public async Task OnShift(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Remove<ChudanPower>(Owner.Creature);
        await PowerCmd.Remove<GedanPower>(Owner.Creature);
        await StanceCmd.Shift(Owner.Creature, Stance.Hasso);
    }
}