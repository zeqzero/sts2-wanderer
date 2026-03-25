using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <summary>
/// Double all Next Turn power amounts
/// </summary>
[Pool(typeof(WandererCardPool))]
public class Keikaku : CustomCardModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ CardKeyword.Exhaust ];

    public Keikaku() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var power in Owner.Creature.Powers)
        {
            if (power is IWandererNextTurnPower && power.Amount != 0)
            {
                await PowerCmd.Apply(power, Owner.Creature, power.Amount, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}