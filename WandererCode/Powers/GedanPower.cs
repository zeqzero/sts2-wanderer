using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Wanderer.WandererCode.Powers;

public class GedanPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DexterityPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("StatGain", 2)];

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        DynamicVars["StatGain"].BaseValue *= Amount;
        await PowerCmd.Apply<GedanDexterityPower>(Owner, DynamicVars["StatGain"].BaseValue, Owner, null);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await PowerCmd.Apply<GedanDexterityPower>(Owner, DynamicVars["StatGain"].BaseValue, Owner, null);
    }
}
