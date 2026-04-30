using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>death, energy</tags>
/// <art>wanderer walking forward holding katana in one hand, arms held out, think Bane vs. PInk Guy meme</art>
/// <kanji>勇</kanji>
[Pool(typeof(WandererCardPool))]
public class Courage : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(2),
        new PowerVar<CouragePower>(20)
    ];

    public Courage() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);

        if (CombatState == null) return;

        var amount = DynamicVars["CouragePower"].BaseValue;
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<CouragePower>(enemy, amount, Owner.Creature, this);
            var power = enemy.GetPower<CouragePower>();
            if (power != null)
            {
                power.Source = Owner.Creature;
            }
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
