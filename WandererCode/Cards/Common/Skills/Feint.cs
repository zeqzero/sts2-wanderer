using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Wanderer.WandererCode.Character;

namespace Wanderer.WandererCode.Cards;

/// <tags>exhaust, counter</tags>
/// <art>wanderer starting an attack, a foe looks alarmed</art>
/// <kanji>虚</kanji>
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
            decimal cardDamage = EstimateDamage(cardModel);
            if (cardDamage > 0m)
            {
                await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(cardDamage, ValueProp.Move), cardPlay);
            }
            await CardCmd.Exhaust(choiceContext, cardModel);
        }
    }

    // Mirror the card's damage preview: pick out whichever damage var the card uses,
    // then run it through the damage hook pipeline (Strength, Vulnerable, enchantments, etc.)
    // against a representative enemy so Feint's block reflects the damage that card would deal.
    private static decimal EstimateDamage(CardModel card)
    {
        var vars = card.DynamicVars;
        Creature? target = card.CombatState?.HittableEnemies.FirstOrDefault();

        decimal baseDamage;
        ValueProp props;
        if (vars.TryGetValue("Damage", out var dv) && dv is DamageVar damageVar)
        {
            baseDamage = damageVar.BaseValue;
            props = damageVar.Props;
        }
        else if (vars.TryGetValue("CalculatedDamage", out var cv) && cv is CalculatedDamageVar calcVar)
        {
            baseDamage = calcVar.Calculate(target);
            props = calcVar.Props;
        }
        else
        {
            return 0m;
        }

        if (baseDamage <= 0m || card.Owner == null) return 0m;

        decimal modified = Hook.ModifyDamage(
            card.Owner.RunState,
            card.CombatState,
            target,
            card.Owner.Creature,
            baseDamage,
            props,
            card,
            ModifyDamageHookType.All,
            CardPreviewMode.Normal,
            out _);

        return Math.Max(0m, modified);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}