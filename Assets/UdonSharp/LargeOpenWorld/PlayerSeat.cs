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
  public Rigidbody InternalRigidBody;

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

  public void Start()
  {
    Tile[0] = 0;
    Tile[1] = 0;
  }

  public void FixedUpdate()
  {
    if (IsOwnedByLocal)
    {
      ConfigureOwned();

      Vector3 currentPosition = Networking.LocalPlayer.GetPosition();
      int tileX = 0;
      int tileY = 0;
      bool moved = false;

      if (Math.Abs(currentPosition.x) > Island.TileLoader.TileSize / 2)
      {
        tileX = Math.Sign(currentPosition.x);
        moved = true;
      }

      if (Math.Abs(currentPosition.z) > Island.TileLoader.TileSize / 2)
      {
        tileY = Math.Sign(currentPosition.z);
        moved = true;
      }

      if (moved)
      {
        Debug.Log($"Tile x:{tileX} y:{tileY}");
        Island.ChangeTilePlayer(this, new Vector2(tileX, tileY));
      }
    }
  }

  public void Update()
  {
    // Move non owned seats to position relative to local player
    /* if (Owner != -1 && !IsOwnedByLocal)
    {
      Vector2 tileOffset = (Tile - Island.TileLoader.CurrentTile) * Island.TileLoader.TileSize;
      GetInteralRigidBody().MovePosition(Position + new Vector3(tileOffset.x, 0, tileOffset.y));
      GetInteralRigidBody().rotation = Rotation;
    } */
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
        Owner = -1;
        RequestSerialization();
      }
    }
    else if (IsOwnedByLocal)
    {
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
    //GetVRCStation().UseStation(Networking.LocalPlayer);
    RequestSerialization();
  }

  public override void OnStationEntered(VRCPlayerApi player)
  {
    Debug.Log($"Seat {gameObject.name} entered by {player.playerId}");

    if (player != Networking.LocalPlayer)
    {
      ConfigureNotOwned();
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

  public void MoveTo(Vector3 position, Vector2 newTile)
  {
    Debug.Log($"Teleport to {position}");
    Tile = newTile;
    Quaternion headRotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
    Networking.LocalPlayer.TeleportTo(position, headRotation);
    ConfigureOwned();
    RequestSerialization();
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
    InternalRigidBody.interpolation = RigidbodyInterpolation.None;
    InternalRigidBody.freezeRotation = true;
    InternalRigidBody.constraints = RigidbodyConstraints.FreezeAll;
    Position = Networking.LocalPlayer.GetPosition();
    Rotation = Networking.LocalPlayer.GetRotation();
    transform.position = Position;
  }

  public void ConfigureNotOwned()
  {
    InternalRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
    GetVRCStation().PlayerMobility = VRCStation.Mobility.ImmobilizeForVehicle;
  }
}
