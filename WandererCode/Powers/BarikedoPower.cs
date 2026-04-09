using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Wanderer.WandererCode.Powers;

public class BarikedoPower : WandererPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldClearBlock(Creature creature)
    {
        if (creature != Owner)
            return true;

        return false;
    }

    public override async Task AfterPreventingBlockClear(AbstractModel preventer, Creature creature)
    {
        if (this != preventer || creature != Owner)
            return;

        int block = creature.Block;
        if (block != 0 && block > Amount)
        {
            await CreatureCmd.LoseBlock(creature, block - Amount);
        }
    }
}
