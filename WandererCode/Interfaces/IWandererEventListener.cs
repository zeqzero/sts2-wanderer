using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Wanderer.WandererCode.Interfaces;

public interface IWandererEventListener
{
    Task BeforeRitualDeath(Creature creature);
    Task AfterEnteredShinigami(Creature creature);
}