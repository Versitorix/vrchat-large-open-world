using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace LargeOpenWorld.Vehicle
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
  public class OpenWorldVehicle : UdonSharpBehaviour
  {
    public GameObject VehicleGameObject;
    public Island Island;

    [UdonSynced]
    public Vector2 VehicleTileLocation = new Vector2(0, 0);

    public bool IsInVehicle { get; private set; } = false;
    public bool IsOwnedByLocal { get; private set; } = false;

    private Rigidbody vehicleRigidBody;

    public void MoveTo(Vector3 position, Vector2 newTile)
    {
      VehicleGameObject.transform.position = position;
      VehicleTileLocation = newTile;

      if (vehicleRigidBody != null)
      {
        vehicleRigidBody.position = position;
      }

      if (IsOwnedByLocal)
        RequestSerialization();
    }

    public void SFEXT_O_PilotEnter()
    {
      Debug.Log("Enter Pilot");
      IsInVehicle = true;
      IsOwnedByLocal = true;
      Island.SetVehicle(this);
      Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    public void SFEXT_O_PilotExit()
    {
      Debug.Log("Exit Pilot");
      Leave();
    }

    public void SFEXT_P_PassengerEnter()
    {
      IsInVehicle = true;
      Island.SetVehicle(this);
    }

    public void SFEXT_P_PassengerExit()
    {
      Leave();
    }

    public void SFEXT_O_RespawnButton()
    {
      if (Networking.IsOwner(gameObject))
      {
        VehicleTileLocation = Vector2.zero;

        RequestSerialization();
      }
    }

    private void Leave()
    {
      IsInVehicle = false;
      IsOwnedByLocal = false;
      Island.RemoveVehicle();
    }
  }
}
