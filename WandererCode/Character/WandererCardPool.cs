using BaseLib.Abstracts;
using Wanderer.WandererCode.Extensions;
using Godot;

namespace Wanderer.WandererCode.Character;

public class WandererCardPool : CustomCardPoolModel
{
    public override string Title => Wanderer.CharacterId; //This is not a display name.

    public override string? BigEnergyIconPath => "res://Wanderer/images/ui/combat/wanderer_energy_icon.png";
    public override string? TextEnergyIconPath => "res://Wanderer/images/ui/combat/text_wanderer_energy_icon.png";


    /* These HSV values will determine the color of your card back.
    They are applied as a shader onto an already colored image,
    so it may take some experimentation to find a color you like.
    Generally they should be values between 0 and 1. */
    public override float H => 0.52f; //Hue; changes the color.
    public override float S => 0.3f; //Saturation
    public override float V => 1.0f; //Brightness

    //Alternatively, leave these values at 1 and provide a custom frame image.
    /*public override Texture2D CustomFrame(CustomCardModel card)
    {
        //This will attempt to load Wanderer/images/cards/frame.png
        return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    }*/

    //Color of small card icons
    public override Color DeckEntryCardColor => Wanderer.Color;

    public override bool IsColorless => false;
}