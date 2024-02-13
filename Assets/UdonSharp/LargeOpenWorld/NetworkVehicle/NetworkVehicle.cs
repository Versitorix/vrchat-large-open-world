using System;
using UdonSharp;
using UnityEngine;
using Varneon.VUdon.ArrayExtensions;
using VRC.SDKBase;

namespace LargeOpenWorld.Vehicle
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
  public class NetworkVehicle : UdonSharpBehaviour
  {
    public GameObject VehicleGameObject;
    public Rigidbody VehicleRigidBody;
    public Island island;
    public IslandTileLoader islandTileLoader;

    [UdonSynced]
    public Vector2 VehicleTileLocation = new Vector2(0, 0);

    public bool IsInVehicle { get; private set; } = false;
    public bool IsOwner { get; private set; } = false;
    private Rigidbody interalRigidBody;
    private BoxCollider interalBoxCollider;

    public void Start()
    {
      interalRigidBody = GetComponent<Rigidbody>();
      interalBoxCollider = GetComponent<BoxCollider>();
    }

    public void FixedUpdate()
    {
      if (!IsInVehicle) return;
  
      if (IsOwner && interalRigidBody != null)
      {
        interalRigidBody.position = VehicleGameObject.transform.position;
      }

      if (!IsOwner && islandTileLoader.CurrentTile != VehicleTileLocation)
      {
        island.ChangeTileVehicle(this, VehicleTileLocation - islandTileLoader.CurrentTile);
      }
    }

    public void EnterPilot()
    {
      IsInVehicle = true;
      IsOwner = true;
      interalBoxCollider.enabled = true;
      island.SetVehicle(this);
      Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    public void EnterPassenger()
    {
      IsInVehicle = true;
      island.SetVehicle(this);
    }

    public void Leave()
    {
      IsInVehicle = false;
      IsOwner = false;
      interalBoxCollider.enabled = false;
      island.RemoveVehicle();
    }

    public void MoveTo(Vector3 position, Vector2 newTile)
    {
      VehicleGameObject.transform.position = position;
      VehicleTileLocation = newTile;

      if (VehicleRigidBody != null)
      {
        VehicleRigidBody.position = position;
      }

      if (IsOwner)
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
