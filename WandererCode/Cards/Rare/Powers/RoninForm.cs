using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

[Pool(typeof(WandererCardPool))]
public class RoninForm : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ new PowerVar<RoninFormPower>(1m) ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var desc = new LocString("powers", "WANDERER-RONIN_FORM_POWER.description");
            desc.Add("Amount", DynamicVars["RoninFormPower"].BaseValue);
            yield return new HoverTip(ModelDb.Power<RoninFormPower>(), desc.GetFormattedText(), false);
        }
    }


    public RoninForm() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<WandererNextTurnRoninFormPower>(Owner.Creature, DynamicVars["RoninFormPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["RoninFormPower"].UpgradeValueBy(1m);
    }
}