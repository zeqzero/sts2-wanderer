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

    public static readonly Color Color = new("0F3548");

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

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets. 
        These are just some of the simplest assets, given some placeholders to differentiate your character with. 
        You don't have to, but you're suggested to rename these images. */
    public override string CustomVisualPath => "res://Wanderer/scenes/wanderer/wanderer.tscn";
    public override string CustomRestSiteAnimPath => "res://Wanderer/scenes/wanderer/wanderer_rest_site.tscn";
    public override string CustomMerchantAnimPath => "res://Wanderer/scenes/wanderer/wanderer_merchant.tscn";
    public override string CustomCharacterSelectBg => "res://Wanderer/scenes/wanderer/char_select_bg_wanderer.tscn";

    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
}