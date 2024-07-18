using BestHTTP.SecureProtocol.Org.BouncyCastle.Security.Certificates;
using LargeOpenWorld.Vehicle;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace LargeOpenWorld
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class Island : UdonSharpBehaviour
  {
    public IslandSlotsManager SlotsManager;
    public IslandTileLoader TileLoader;
    public IslandSlot CurrentSlot;

    private NetworkVehicle playerVehicle;
    private bool UpdateTileNextTick = false;

    public void FixedUpdate()
    {
      if (UpdateTileNextTick == true)
      {
        UpdateTileNextTick = false;
        ValidateCurrentSlotOrChange();
      }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
      CurrentSlot.AddPlayer(player);
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
      CurrentSlot.RemovePlayer(player);
    }

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
      if (player.isLocal && TileLoader.CurrentTile != Vector2.zero)
      {
        TileLoader.ChangeTile(Vector2.zero - TileLoader.CurrentTile);
        EnterSlot(SlotsManager.GetJoinableIslandForTile(Vector2.zero));
      }
    }

    public void EnterSlot(IslandSlot slot)
    {
      if (Networking.IsOwner(CurrentSlot.gameObject))
      {
        CurrentSlot.Leave();
      }

      CurrentSlot = slot;
      transform.position = slot.transform.position;

      if (slot.Tile.Length == 0)
      {
        CurrentSlot.EnterAndTakeOwner(TileLoader.CurrentTile, this);
      }
    }

    /// <summary>
    /// Move player to a new tile.
    /// </summary>
    /// <param name="direction"></param>
    public void ChangeTilePlayer(Vector2 direction)
    {
      Vector3 nextPosition = UpdateTileWithDirection(Networking.LocalPlayer.GetPosition(), direction);
      Quaternion headRotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
      Networking.LocalPlayer.TeleportTo(nextPosition, headRotation);
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

    public void HandleIslandOwnershipLost()
    {
      
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
      Vector3 localNextPosition = transform.InverseTransformPoint(
        new Vector3(
          position.x - TileLoader.TileSize * direction.x,
          position.y,
          position.z - TileLoader.TileSize * direction.y
        )
      );

      TileLoader.ChangeTile(direction);
      IslandSlot existingSlot = SlotsManager.GetJoinableIslandForTile(TileLoader.CurrentTile);

      if (existingSlot != null)
      {
        EnterSlot(existingSlot);
      }
      else if (!Networking.IsOwner(CurrentSlot.gameObject) || CurrentSlot.Players.Length >= 1)
      {
        EnterSlot(SlotsManager.GetFreeSlot());
      }
      else
      {
        CurrentSlot.UpdateTile(TileLoader.CurrentTile);
      }

      return transform.TransformPoint(localNextPosition);
    }

    private void ValidateCurrentSlotOrChange()
    {
      if (
        CurrentSlot.Tile[0] != (int)TileLoader.CurrentTile.x
        || CurrentSlot.Tile[1] != (int)TileLoader.CurrentTile.y
      )
      {
        if (playerVehicle == null)
        {
          Vector3 nextPosition = UpdateTileWithDirection(Networking.LocalPlayer.GetPosition(), Vector2.zero);
          Quaternion headRotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
          Networking.LocalPlayer.TeleportTo(nextPosition, headRotation);
        }
        else
        {
          Vector3 nextPosition = UpdateTileWithDirection(playerVehicle.gameObject.transform.position, Vector2.zero);

          playerVehicle.MoveTo(nextPosition, TileLoader.CurrentTile);
        }
      }
    }
  }
}
