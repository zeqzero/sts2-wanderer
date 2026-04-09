using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Commands;
using Wanderer.WandererCode.Interfaces;

namespace Wanderer.WandererCode.Powers;

public class ChudanPower : WandererPower, IStancePower
{
    public Stance Stance => Stance.Chudan;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ HoverTipFactory.FromPower<ChudanStrengthPower>() ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ChudanStrengthPower>(2)];

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await PowerCmd.Apply<ChudanStrengthPower>(Owner, DynamicVars["ChudanStrengthPower"].BaseValue * Amount, Owner, null);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await PowerCmd.Apply<ChudanStrengthPower>(Owner, DynamicVars["ChudanStrengthPower"].BaseValue * Amount, Owner, null);
    }
}
