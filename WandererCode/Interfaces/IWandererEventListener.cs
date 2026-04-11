using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using Wanderer.WandererCode.Commands;

namespace Wanderer.WandererCode.Interfaces;

public interface IWandererEventListener
{
    Task BeforeRitualDeath(Creature creature);
    Task AfterEnteredShinigami(Creature creature);
    Task AfterStanceLeft(Creature creature, Stance oldStance);
    Task AfterStanceEntered(Creature creature, Stance stance);
    Task AfterShifted(CardModel card);
    Task AfterRefilled(CardModel card);
}