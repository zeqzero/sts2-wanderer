using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>nextturn</tags>
/// <art>wanderer sitting at a desk writing, think "I'll take a potato chip AND EAT IT", arm held out with ink brush in hand</art>
/// <kanji>計</kanji>
[Pool(typeof(WandererCardPool))]
public class Keikaku : WandererCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public Keikaku() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var power in Owner.Creature.Powers)
        {
            if (NextTurnPowers.Is(power) && power.Amount != 0)
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