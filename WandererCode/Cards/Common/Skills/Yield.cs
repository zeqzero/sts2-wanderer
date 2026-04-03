using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>stance</tags>
[Pool(typeof(WandererCardPool))]
public class Yield : WandererCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move), new CardsVar(1)];

    public Yield() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        // Check if Shift is already in hand
        if (PileType.Hand.GetPile(Owner).Cards.Any(c => c is Shift))
            return;

        // Search draw, discard, and exhaust for an existing Shift
        CardModel? shift = PileType.Draw.GetPile(Owner).Cards.FirstOrDefault(c => c is Shift)
            ?? PileType.Discard.GetPile(Owner).Cards.FirstOrDefault(c => c is Shift)
            ?? PileType.Exhaust.GetPile(Owner).Cards.FirstOrDefault(c => c is Shift);

        if (shift == null)
        {
            shift = CombatState.CreateCard<Shift>(Owner);
        }
        await CardPileCmd.Add(shift, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}