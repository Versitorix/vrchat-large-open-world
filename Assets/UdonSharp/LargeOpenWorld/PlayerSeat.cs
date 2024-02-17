using System;
using LargeOpenWorld;
using UdonSharp;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SocialPlatforms;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class PlayerSeat : UdonSharpBehaviour
{
  public Island Island;
  public PlayerSeatsManager SeatsManager;

  [UdonSynced]
  public int Owner = -1;
  [UdonSynced]
  public Vector2 Tile = Vector2.zero;
  [UdonSynced]
  public Vector3 Position = Vector3.zero;
  [UdonSynced]
  public Quaternion Rotation = Quaternion.identity;

  public bool IsOwnedByLocal { get; private set; } = false;

  private VRCStation station;
  private bool initialized = false;
  private Vector3 velocity = Vector3.zero;

  public override void PostLateUpdate()
  {
    if (IsOwnedByLocal)
    {
      Position = Networking.LocalPlayer.GetPosition();
      Rotation = Networking.LocalPlayer.GetRotation();
      transform.SetPositionAndRotation(Position, Rotation);
    }
  }

  void Update()
  {
    if (Owner == -1 || IsOwnedByLocal) return;

    UpdatePositionWithNetworkData();
  }

  public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
  {
    if (IsOwnedByLocal && requestingPlayer != Networking.LocalPlayer)
    {
      return false;
    }

    if (requestedOwner == Networking.LocalPlayer)
    {
      Owner = requestedOwner.playerId;
    }

    return true;
  }

  public override void OnOwnershipTransferred(VRCPlayerApi player)
  {
    if (player.isLocal)
    {
      if (Owner == player.playerId)
      {
        FinaliseOwnership();
      }
      else
      {
        // Reset seat. Recieved ownership without requesting, probably a player disconnect.
        Owner = -1;
        RequestSerialization();
      }
    }
    else if (IsOwnedByLocal)
    {
      //Ownership lost
      IsOwnedByLocal = false;
      ConfigureNotOwned();
      SeatsManager.ClearLocalSeat();
    }
  }

  public void FinaliseOwnership()
  {
    Debug.Log("Finishing ownership");
    Owner = Networking.LocalPlayer.playerId;
    IsOwnedByLocal = true;
    ConfigureOwned();
    GetVRCStation().UseStation(Networking.LocalPlayer);
    RequestSerialization();
  }

  public override void OnStationEntered(VRCPlayerApi player)
  {
    Debug.Log($"Seat {gameObject.name} entered by {player.playerId}");

    if (player != Networking.LocalPlayer)
    {
      ConfigureNotOwned();
    }
    else
    {
      ConfigureOwned();
    }
  }

  public override void OnStationExited(VRCPlayerApi player)
  {
    Debug.Log($"Seat {gameObject.name} exited by {player.playerId}");

    if (player == Networking.LocalPlayer && Owner == -1)
    {
      ConfigureNotOwned();
    }
  }

  public override void OnDeserialization()
  {
    base.OnDeserialization();

    if (!initialized)
    {
      SeatsManager.SeatReady();
      initialized = true;
    }
  }

  public void DispatchTileChange(Vector2 newTile)
  {
    Tile = newTile;
    ConfigureOwned();
    GetVRCStation().UseStation(Networking.LocalPlayer);
    RequestSerialization();
  }

  public void UpdatePositionWithNetworkData(bool skipInterpolate = false)
  {
    Vector2 tileOffset = (Tile - Island.TileLoader.GetLatestTile()) * Island.TileLoader.TileSize;

    if (skipInterpolate)
    {
      transform.position = Position + new Vector3(tileOffset.x, 0, tileOffset.y);
      transform.rotation = Rotation;
    }
    else 
    {
      transform.position = Vector3.Lerp(transform.position, Position + new Vector3(tileOffset.x, 0, tileOffset.y), 0.05f);
      transform.rotation = Quaternion.Slerp(transform.rotation, Rotation, 0.05f);
    }
  }

  private VRCStation GetVRCStation()
  {
    if (station == null)
      station = (VRCStation)GetComponent(nameof(VRCStation));

    return station;
  }

  private void ConfigureOwned()
  {
    GetVRCStation().PlayerMobility = VRCStation.Mobility.Mobile;
    Position = Networking.LocalPlayer.GetPosition();
    Rotation = Networking.LocalPlayer.GetRotation();
    transform.SetPositionAndRotation(Position, Rotation);
  }

  public void ConfigureNotOwned()
  {
    GetVRCStation().PlayerMobility = VRCStation.Mobility.ImmobilizeForVehicle;
  }
}
