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

        // Check if Kamae is already in hand
        if (PileType.Hand.GetPile(Owner).Cards.Any(c => c is Kamae))
            return;

        // Search draw, discard, and exhaust for an existing Kamae
        CardModel? kamae = PileType.Draw.GetPile(Owner).Cards.FirstOrDefault(c => c is Kamae)
            ?? PileType.Discard.GetPile(Owner).Cards.FirstOrDefault(c => c is Kamae)
            ?? PileType.Exhaust.GetPile(Owner).Cards.FirstOrDefault(c => c is Kamae);

        if (kamae == null)
        {
            kamae = CombatState.CreateCard<Kamae>(Owner);
        }
        await CardPileCmd.Add(kamae, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}