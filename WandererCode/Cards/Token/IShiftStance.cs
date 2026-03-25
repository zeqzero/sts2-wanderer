using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

public interface IShiftStance
{
    public Task OnShift(PlayerChoiceContext choiceContext, CardPlay cardPlay);
}