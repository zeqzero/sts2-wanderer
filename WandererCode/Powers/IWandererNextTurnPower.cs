using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

public interface IWandererNextTurnPower
{
    public Task ApplyNow(PlayerChoiceContext choiceContext, Player player);
}