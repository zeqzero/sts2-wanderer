using BaseLib.Abstracts;
using Wanderer.WandererCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Cards;
using Wanderer.WandererCode.Relics;

namespace Wanderer.WandererCode.Character;

public class Wanderer : PlaceholderCharacterModel
{
    public const string CharacterId = "Wanderer";

    public static readonly Color Color = new("3E5158");
    public override Color MapDrawingColor => Color;

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 50;

    //public override IEnumerable<CardModel> StartingDeck => Enumerable.Range(0, 10).Select(_ => ModelDb.Card<Seppuku>());
    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<Kamae>(),
        ModelDb.Card<StrikeWanderer>(),
        ModelDb.Card<StrikeWanderer>(),
        ModelDb.Card<StrikeWanderer>(),
        ModelDb.Card<StrikeWanderer>(),
        ModelDb.Card<StrikeWanderer>(),
        ModelDb.Card<DefendWanderer>(),
        ModelDb.Card<DefendWanderer>(),
        ModelDb.Card<DefendWanderer>(),
        ModelDb.Card<DefendWanderer>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<BrokenJuzuRelic>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<WandererCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<WandererRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<WandererPotionPool>();

    public override string CustomVisualPath => "res://Wanderer/scenes/wanderer/wanderer.tscn";
    public override string CustomRestSiteAnimPath => "res://Wanderer/scenes/wanderer/wanderer_rest_site.tscn";
    public override string CustomMerchantAnimPath => "res://Wanderer/scenes/wanderer/wanderer_merchant.tscn";
    public override string CustomCharacterSelectBg => "res://Wanderer/scenes/wanderer/char_select_bg_wanderer.tscn";
    public override string CustomIconPath => "res://Wanderer/scenes/wanderer/wanderer_icon.tscn";

    public override string CustomIconTexturePath => "res://Wanderer/images/wanderer/character_icon_wanderer.png";
    public override string CustomCharacterSelectIconPath => "res://Wanderer/images/wanderer/char_select_wanderer.png";
    public override string CustomCharacterSelectLockedIconPath => "res://Wanderer/images/wanderer/char_select_wanderer_locked.png";
    public override string CustomMapMarkerPath => "res://Wanderer/images/wanderer/map_marker_wanderer.png";

    public override string CustomArmPointingTexturePath => "res://Wanderer/images/wanderer/hands/multiplayer_hand_wanderer_point.png";
    public override string CustomArmRockTexturePath => "res://Wanderer/images/wanderer/hands/multiplayer_hand_wanderer_rock.png";
    public override string CustomArmPaperTexturePath => "res://Wanderer/images/wanderer/hands/multiplayer_hand_wanderer_paper.png";
    public override string CustomArmScissorsTexturePath => "res://Wanderer/images/wanderer/hands/multiplayer_hand_wanderer_scissors.png";

    public override CustomEnergyCounter? CustomEnergyCounter => new CustomEnergyCounter(
        i => "res://Wanderer/images/ui/combat/energy_counters/wanderer_orb_layer_" + i + ".png", 
        new Color("8BB5C4"), 
        new Color("8BB5C4"));
}