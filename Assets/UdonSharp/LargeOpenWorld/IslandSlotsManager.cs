using UdonSharp;
using UnityEngine;

namespace LargeOpenWorld {
  public class IslandSlotsManager : UdonSharpBehaviour
  {
    public IslandSlot[] IslandSlots;

    public IslandSlot GetJoinableIslandForTile(Vector2 tile)
    {
      foreach (IslandSlot slot in IslandSlots)
      {
        if (
          slot.Tile.Length == 2
          && slot.Tile[0] == (int)tile.x
          && slot.Tile[1] == (int)tile.y
        )
        {
          return slot;
        }
      }
    
      return null;
    }

    public IslandSlot GetFreeSlot()
    {
      foreach (IslandSlot slot in IslandSlots)
      {
        if (slot.Tile.Length == 0)
        {
          return slot;
        }
      }

      return null;
    }

    public IslandSlot GetPriorityIsland(IslandSlot currentSlot)
    {
      foreach (IslandSlot slot in IslandSlots)
      {
        if (
          slot.Tile.Length == 2
          && slot.Tile[0] == currentSlot.Tile[0]
          && slot.Tile[1] == currentSlot.Tile[1]
          && slot.PrioritySeed > currentSlot.PrioritySeed
        )
        {
          return slot;
        }
      }

      return null;
    }
  }
}
