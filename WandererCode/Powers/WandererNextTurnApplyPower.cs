using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Powers;

public abstract class WandererNextTurnApplyPower<T> : WandererPower, IWandererNextTurnPower where T : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var power = ModelDb.Power<T>();
            var desc = new LocString("powers", power.Id.Entry + ".description");
            desc.Add("Amount", Amount);
            yield return new HoverTip(power, desc.GetFormattedText(), false);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar("Power", ModelDb.Power<T>().Title.GetFormattedText())];

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == Owner.Side && AmountOnTurnStart != 0)
        {
            await PowerCmd.Apply<T>(Owner, Amount, Owner, null);
            if (!WandererCmd.ShouldPreserveNextTurnPowers(Owner))
            {
                await PowerCmd.Remove(this);
            }
        }
    }

    public async Task ApplyNow(PlayerChoiceContext choiceContext, Player player)
    {
        await PowerCmd.Apply<T>(Owner, Amount, Owner, null);
        if (!WandererCmd.ShouldPreserveNextTurnPowers(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}