using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace LargeOpenWorld.Vehicle
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
  [DefaultExecutionOrder(20)]
  public class NetworkVehicle : UdonSharpBehaviour
  {
    public GameObject VehicleGameObject;
    public Rigidbody VehicleRigidBody;
    public Island Island;

    [UdonSynced]
    public Vector2 VehicleTileLocation = new Vector2(0, 0);

    public bool IsInVehicle { get; private set; } = false;
    public bool IsOwnedByLocal { get; private set; } = false;

    public override void PostLateUpdate()
    {
      if (!IsOwnedByLocal)
      {
        Vector2 tileOffset = (VehicleTileLocation - Island.TileLoader.CurrentTile) * Island.TileLoader.TileSize;
        VehicleGameObject.transform.position = VehicleGameObject.transform.position + new Vector3(tileOffset.x, 0, tileOffset.y);
        VehicleRigidBody.position = VehicleRigidBody.position + new Vector3(tileOffset.x, 0, tileOffset.y);
      }
    }

    public void EnterPilot()
    {
      IsInVehicle = true;
      IsOwnedByLocal = true;
      Island.SetVehicle(this);
      Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    public void EnterPassenger()
    {
      IsInVehicle = true;
      Island.SetVehicle(this);
    }

    public void Leave()
    {
      IsInVehicle = false;
      IsOwnedByLocal = false;
      Island.RemoveVehicle();
    }

    public void MoveTo(Vector3 position, Vector2 newTile)
    {
      VehicleGameObject.transform.position = position;
      VehicleTileLocation = newTile;

      if (VehicleRigidBody != null)
      {
        VehicleRigidBody.position = position;
      }

      if (IsOwnedByLocal)
        RequestSerialization();
    }

    public void SFEXT_O_RespawnButton()
    {
      if (Networking.IsOwner(gameObject))
      {
        VehicleTileLocation = Vector2.zero;

        RequestSerialization();
      }
    }
  }
}
