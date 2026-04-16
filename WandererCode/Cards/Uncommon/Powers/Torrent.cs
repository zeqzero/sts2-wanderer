using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Wanderer.WandererCode.Character;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Cards;

/// <tags>dance</tags>
/// <art>wanderer in hasso stance, crashing water background, tight zoom upper body</art>
/// <kanji>激</kanji>
[Pool(typeof(WandererCardPool))]
public class Torrent : WandererCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<TorrentPower>(3)];

    protected override IEnumerable<IHoverTip> WandererExtraHoverTips =>
    [
    ];

    public Torrent() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<TorrentPower>(Owner.Creature, DynamicVars["TorrentPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["TorrentPower"].UpgradeValueBy(2m);
    }
}