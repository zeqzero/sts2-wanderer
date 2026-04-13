using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

public interface IEnterStance
{
    public Task OnEnter(PlayerChoiceContext choiceContext, CardPlay cardPlay, int amount);
}