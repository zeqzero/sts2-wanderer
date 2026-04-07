using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Wanderer.WandererCode.Powers;

public class GedanPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(3, ValueProp.Unpowered)];

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await CreatureCmd.GainBlock(Owner, DynamicVars.Block.BaseValue, ValueProp.Unpowered, null);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await CreatureCmd.GainBlock(Owner, Amount * DynamicVars.Block.BaseValue, ValueProp.Unpowered, null);
    }
}
