using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Wanderer.WandererCode.Powers;

namespace Wanderer.WandererCode.Commands;

public enum Stance
{
    Chudan,
    Hasso,
    Gedan,
    Jodan,
    Waki
}

public static class StanceCmd
{
    public static event Func<Creature, Stance, Task>? Shifted;

    public static async Task Shift(Creature creature, Stance stance)
    {
        switch (stance)
        {
            case Stance.Chudan:
                await PowerCmd.Apply<ChudanPower>(creature, 1, creature, null);
                break;
            case Stance.Hasso:
                await PowerCmd.Apply<HassoPower>(creature, 1, creature, null);
                break;
            case Stance.Gedan:
                await PowerCmd.Apply<GedanPower>(creature, 1, creature, null);
                break;
        }

        if (Shifted != null)
        {
            await Shifted.Invoke(creature, stance);
        }
    }
}