using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Wanderer.WandererCode.Nodes;
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

    public static bool JodanEnabled = false;

    public static bool WakiEnabled = false;

    private static readonly Dictionary<Creature, int> _shiftCounts = new();

    public static int GetShiftCount(Creature creature) =>
        _shiftCounts.TryGetValue(creature, out var count) ? count : 0;

    public static void Reset()
    {
        _shiftCounts.Clear();
        JodanEnabled = false;
        WakiEnabled = false;
    }

    public static async Task Shift(Creature creature, Stance stance)
    {
        // Remove all existing stance powers
        await PowerCmd.Remove<ChudanPower>(creature);
        await PowerCmd.Remove<HassoPower>(creature);
        await PowerCmd.Remove<GedanPower>(creature);
        await PowerCmd.Remove<JodanPower>(creature);
        await PowerCmd.Remove<WakiPower>(creature);

        // Apply the new stance
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
            case Stance.Jodan:
                await PowerCmd.Apply<JodanPower>(creature, 1, creature, null);
                JodanEnabled = true;
                break;
            case Stance.Waki:
                await PowerCmd.Apply<WakiPower>(creature, 1, creature, null);
                WakiEnabled = true;
                break;
        }

        _shiftCounts[creature] = GetShiftCount(creature) + 1;

        WandererVisuals.SetStance(creature, stance.ToString().ToLowerInvariant());

        if (Shifted != null)
        {
            await Shifted.Invoke(creature, stance);
        }
    }
}