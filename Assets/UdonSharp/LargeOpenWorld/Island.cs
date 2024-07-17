using System;
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

    private OpenWorldVehicle playerVehicle;
  
    private bool inZone = false;

    void Update()
    {
      Vector3 currentPosition = Networking.LocalPlayer.GetPosition();
      int tileX = 0;
      int tileY = 0;
      bool inZoneThisFrame = false;

      if (Math.Abs(currentPosition.x) > TileLoader.TileSize / 2)
      {
        tileX = Math.Sign(currentPosition.x);
        inZoneThisFrame = true;
      }

      if (Math.Abs(currentPosition.z) > TileLoader.TileSize / 2)
      {
        tileY = Math.Sign(currentPosition.z);
        inZoneThisFrame = true;
      }

      if (!inZoneThisFrame && inZone)
      {
        inZone = false;
      }
      else if (inZoneThisFrame && !inZone)
      {
        inZone = true;
        if (playerVehicle == null)
        {
          ChangeTilePlayer(new Vector2Int(tileX, tileY));
        }
        else
        {
          ChangeTileVehicle(new Vector2Int(tileX, tileY));
        }
      }
    }

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
      if (player.isLocal && TileLoader.GetLatestTile() != Vector2.zero)
      {
        TileLoader.MoveInDirection(Vector2Int.zero - TileLoader.GetLatestTile());
      }
    }

    /// <summary>
    /// Move player to a new tile.
    /// </summary>
    /// <param name="direction"></param>
    public void ChangeTilePlayer(Vector2Int direction)
    {
      Vector3 playerPosition = Networking.LocalPlayer.GetPosition();
      Quaternion headRotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
      Vector3 nextPosition = UpdateTileWithDirection(playerPosition, direction);
      Networking.LocalPlayer.TeleportTo(nextPosition, headRotation);
      SeatsManager.ProcessPlayerChangedTile(TileLoader.GetLatestTile());
    }

    /// <summary>
    /// Move controlled vehicle to a new tile.
    /// </summary>
    /// <param name="vehicle"></param>
    /// <param name="direction"></param>
    public void ChangeTileVehicle(Vector2Int direction)
    {
      Vector3 nextPosition = UpdateTileWithDirection(playerVehicle.VehicleGameObject.transform.position, direction);
      playerVehicle.MoveTo(nextPosition, TileLoader.GetLatestTile());
    }

    public void SetVehicle(OpenWorldVehicle vehicle)
    {
      Debug.Log("Entering vehicle");
      playerVehicle = vehicle;
    }

    public void RemoveVehicle()
    {
      playerVehicle = null;
      SeatsManager.ProcessPlayerChangedTile(TileLoader.GetLatestTile());
    }

    /// <summary>
    /// Change Tile, IslandSlot and calculate next position based on direction.
    /// </summary>
    /// <param name="position">Position of the current entity</param>
    /// <param name="direction">Direction of movement</param>
    /// <returns>Next position for the entity</returns>
    private Vector3 UpdateTileWithDirection(Vector3 position, Vector2Int direction)
    {
      TileLoader.MoveInDirection(direction);

      return new Vector3(
        position.x - TileLoader.TileSize * direction.x,
        position.y,
        position.z - TileLoader.TileSize * direction.y
      );
    }
  }
}
