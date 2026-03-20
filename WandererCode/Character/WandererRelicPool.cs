using BaseLib.Abstracts;
using Wanderer.WandererCode.Extensions;
using Godot;

namespace Wanderer.WandererCode.Character;

public class WandererRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => Wanderer.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}