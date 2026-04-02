using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Cards;

[Pool(typeof(WandererCardPool))]
public class BarrelRoll : WandererCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [ new BlockVar(8, ValueProp.Move) ];

    public BarrelRoll() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // gain block, transform a card randomly
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (IsUpgraded)
        {
            var prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1);
            var card = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this)).FirstOrDefault();

            if (card != null)
            {
                await WandererCmd.TransformToRandomFromPool(card, Owner);
            }
        }
        else
        {
            var pile = PileType.Hand.GetPile(Owner);
            var card = Owner.RunState.Rng.CombatCardSelection.NextItem(pile.Cards);
            if (card != null)
            {
                await WandererCmd.TransformToRandomFromPool(card, Owner);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}