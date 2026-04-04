using MegaCrit.Sts2.Core.Entities.Creatures;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Interfaces;

public interface IWandererEventListener
{
    Task BeforeRitualDeath(Creature creature);
    Task AfterEnteredShinigami(Creature creature);
    Task AfterStanceEntered(Creature creature, Stance stance);
}