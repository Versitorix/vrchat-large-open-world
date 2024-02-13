using UdonSharp;
using UnityEngine;
using Varneon.VUdon.ArrayExtensions;
using VRC.SDKBase;

namespace LargeOpenWorld
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
  public class IslandSlot : UdonSharpBehaviour
  {
    [UdonSynced]
    public int[] Tile = new int[0];
    [UdonSynced]
    public int[] Players = new int[0];
    [UdonSynced]
    public int PrioritySeed;

    public IslandSlotsManager WorldIslandsManager;

    private Vector2 nextTile = new Vector2(0.1f, 0.1f);
    private Island localIsland;

    public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
    {
      if (requestingPlayer == Networking.LocalPlayer)
        return true;

      if (requestedOwner == requestingPlayer && (Tile.Length == 0 || (Tile[0] == 0 && Tile[1] == 0)))
        return true;

      return false;
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
      if (player.isLocal && nextTile.x != 0.1f)
      {
        Tile = new int[2];
        Tile[0] = (int)nextTile.x;
        Tile[1] = (int)nextTile.y;
        PrioritySeed = Random.Range(0, 320);
        nextTile = new Vector2(0.1f, 0.1f);

        RequestSerialization();
      }

      if (!player.isLocal && localIsland != null)
      {
        localIsland.HandleIslandOwnershipLost();
        localIsland = null;
      }
    }

    public void Leave()
    {
      // Reset tile if last player
      if (Players.Length == 0)
      {
        Tile = new int[0];
        Players = new int[0];
      }
      else if (Tile[0] != 0 && Tile[1] != 0)
      {
        Players = Players.Remove(VRCPlayerApi.GetPlayerId(Networking.LocalPlayer));
        Networking.SetOwner(VRCPlayerApi.GetPlayerById(Players[1]), gameObject);
      }

      localIsland = null;
      RequestSerialization();
    }

    public void EnterAndTakeOwner(Vector2 tile, Island island)
    {
      nextTile = tile;
      localIsland = island;
    
      // Handle session owner case
      if (Networking.IsOwner(gameObject))
      {
        OnOwnershipTransferred(Networking.LocalPlayer);
      }
      else
      {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
      }
    }

    public void UpdateTile(Vector2 tile)
    {
      Tile[0] = (int)tile.x;
      Tile[1] = (int)tile.y;

      RequestSerialization();
    }

    public void AddPlayer(VRCPlayerApi player)
    {
      if (Networking.IsOwner(gameObject))
      {
        Players = Players.AddUnique(VRCPlayerApi.GetPlayerId(player));
        RequestSerialization();
      }
    }

    public void RemovePlayer(VRCPlayerApi player)
    {
      if (Networking.IsOwner(gameObject))
      {
        Players = Players.Remove(VRCPlayerApi.GetPlayerId(player));
        RequestSerialization();
      }
    }
  }
}
