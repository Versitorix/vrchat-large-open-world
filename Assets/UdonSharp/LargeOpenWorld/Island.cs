using LargeOpenWorld.Vehicle;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace LargeOpenWorld
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class Island : UdonSharpBehaviour
  {
    public IslandTileLoader TileLoader;
    public PlayerSeatsManager SeatsManager;

    private NetworkVehicle playerVehicle;

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
      if (player.isLocal && TileLoader.CurrentTile != Vector2.zero)
      {
        TileLoader.QueueTileChange(Vector2.zero - TileLoader.CurrentTile);
      }
    }

    /// <summary>
    /// Move player to a new tile.
    /// </summary>
    /// <param name="direction"></param>
    public void ChangeTilePlayer(PlayerSeat playerSeat, Vector2 direction)
    {
      Vector3 playerPosition = Networking.LocalPlayer.GetPosition();
      Vector3 nextPosition = new Vector3(
        playerPosition.x - TileLoader.TileSize * direction.x,
        playerPosition.y + 0.1f,
        playerPosition.z - TileLoader.TileSize * direction.y
      );
      
      if (TileLoader.QueueTileChange(direction))
      {
        playerSeat.MoveTo(nextPosition, TileLoader.NextTile);
      }
    }

    /// <summary>
    /// Move controlled vehicle to a new tile.
    /// </summary>
    /// <param name="vehicle"></param>
    /// <param name="direction"></param>
    public void ChangeTileVehicle(NetworkVehicle vehicle, Vector2 direction)
    {
      Vector3 nextPosition = UpdateTileWithDirection(vehicle.VehicleGameObject.transform.position, direction);
      vehicle.MoveTo(nextPosition, TileLoader.CurrentTile);
    }

    public void ChangeTileNoTeleport(Vector2 direction)
    {
      UpdateTileWithDirection(Vector3.zero, direction);
    }

    public void SetVehicle(NetworkVehicle vehicle)
    {
      playerVehicle = vehicle;
    }

    public void RemoveVehicle()
    {
      playerVehicle = null;
    }

    /// <summary>
    /// Change Tile, IslandSlot and calculate next position based on direction.
    /// </summary>
    /// <param name="position">Position of the current entity</param>
    /// <param name="direction">Direction of movement</param>
    /// <returns>Next position for the entity</returns>
    private Vector3 UpdateTileWithDirection(Vector3 position, Vector2 direction)
    {
      if (!TileLoader.QueueTileChange(direction))
      {
        return position;
      }

      return transform.TransformPoint(
        new Vector3(
          position.x - TileLoader.TileSize * direction.x,
          position.y,
          position.z - TileLoader.TileSize * direction.y
        )
      );
    }
  }
}
