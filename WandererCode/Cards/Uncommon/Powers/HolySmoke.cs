using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>exhaust</tags>
[Pool(typeof(WandererCardPool))]
public class HolySmoke : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<HolySmokePower>(3)];

    public HolySmoke() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<HolySmokePower>(Owner.Creature, DynamicVars["HolySmokePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HolySmokePower"].UpgradeValueBy(2m);
    }
}