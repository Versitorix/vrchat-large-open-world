using System;
using LargeOpenWorld.Vehicle;
using UdonSharp;
using UnityEngine;
using Varneon.VUdon.ArrayExtensions;
using VRC.SDKBase;

namespace LargeOpenWorld
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class ExitBounds : UdonSharpBehaviour
  {
    [SerializeField]
    private Island Island;

    public override void OnPlayerTriggerStay(VRCPlayerApi player)
    {
      if (player.isLocal == true && Island != null)
      {
        Vector3 currentPosition = transform.parent.InverseTransformPoint(player.GetPosition());
        int tileX = 0;
        int tileY = 0;

        if (Math.Abs(currentPosition.x) > Island.TileLoader.TileSize / 2)
          tileX = Math.Sign(currentPosition.x);

        if (Math.Abs(currentPosition.z) > Island.TileLoader.TileSize / 2)
          tileY = Math.Sign(currentPosition.z);

        Island.ChangeTilePlayer(new Vector2(tileX, tileY));
      }
    }

    public void OnTriggerStay(Collider collider)
    {
      NetworkVehicle vehicle = collider.gameObject.GetComponent<NetworkVehicle>();

      if (vehicle != null && vehicle.IsOwner)
      {
        Vector3 currentPosition = transform.parent.InverseTransformPoint(collider.gameObject.transform.position);
        int tileX = 0;
        int tileY = 0;

        if (Math.Abs(currentPosition.x) > Island.TileLoader.TileSize / 2)
          tileX = Math.Sign(currentPosition.x);

        if (Math.Abs(currentPosition.z) > Island.TileLoader.TileSize / 2)
          tileY = Math.Sign(currentPosition.z);

        Island.ChangeTileVehicle(vehicle, new Vector2(tileX, tileY));
      }
    }
  }
}
