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

    private NetworkVehicle playerVehicle;
  
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
          ChangeTilePlayer(new Vector2(tileX, tileY));
        }
        else
        {
          ChangeTileVehicle(new Vector2(tileX, tileY));
        }
      }
    }

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
    public void ChangeTilePlayer(Vector2 direction)
    {
      Vector3 playerPosition = Networking.LocalPlayer.GetPosition();
      Quaternion headRotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
      Vector3 nextPosition = UpdateTileWithDirection(playerPosition, direction);
      Networking.LocalPlayer.TeleportTo(nextPosition, headRotation);
      SeatsManager.LocalSeat.MoveTo(TileLoader.NextTile);
    }

    /// <summary>
    /// Move controlled vehicle to a new tile.
    /// </summary>
    /// <param name="vehicle"></param>
    /// <param name="direction"></param>
    public void ChangeTileVehicle(Vector2 direction)
    {
      Vector3 nextPosition = UpdateTileWithDirection(playerVehicle.VehicleGameObject.transform.position, direction);
      playerVehicle.MoveTo(nextPosition, TileLoader.CurrentTile);
    }

    public void ChangeTileNoTeleport(Vector2 direction)
    {
      UpdateTileWithDirection(Vector3.zero, direction);
    }

    public void SetVehicle(NetworkVehicle vehicle)
    {
      Debug.Log("Entering vehicle");
      playerVehicle = vehicle;
    }

    public void RemoveVehicle()
    {
      playerVehicle = null;
      SeatsManager.LocalSeat.MoveTo(TileLoader.CurrentTile);
    }

    /// <summary>
    /// Change Tile, IslandSlot and calculate next position based on direction.
    /// </summary>
    /// <param name="position">Position of the current entity</param>
    /// <param name="direction">Direction of movement</param>
    /// <returns>Next position for the entity</returns>
    private Vector3 UpdateTileWithDirection(Vector3 position, Vector2 direction)
    {
      TileLoader.QueueTileChange(direction);

      return new Vector3(
        position.x - TileLoader.TileSize * direction.x,
        position.y,
        position.z - TileLoader.TileSize * direction.y
      );
    }
  }
}
