using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>exhaust, counter</tags>
[Pool(typeof(WandererCardPool))]
public class Feint : WandererCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public Feint() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? cardModel = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), context: choiceContext, player: base.Owner, filter: c => c.Type == CardType.Attack, source: this)).FirstOrDefault();
        if (cardModel != null)
        {
            var cardDamage = cardModel.DynamicVars.Damage.BaseValue;
            if (cardDamage > 0)
            {
                await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(cardDamage, ValueProp.Move), cardPlay);
            }
            await CardCmd.Exhaust(choiceContext, cardModel);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}