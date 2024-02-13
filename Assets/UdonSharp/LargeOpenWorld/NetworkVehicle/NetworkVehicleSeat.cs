
using System;
using LargeOpenWorld;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace LargeOpenWorld.Vehicle
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class NetworkVehicleSeat : UdonSharpBehaviour
    {
        public NetworkVehicle vehicle;
        public bool IsPilot = false;

        public override void OnStationEntered(VRCPlayerApi player)
        {
            if (!player.isLocal) return;

            if (IsPilot)
            {
                vehicle.EnterPilot();
            }
            else
            {
                vehicle.EnterPassenger();
            }
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            if (!player.isLocal) return;

            vehicle.Leave();
        }
    }

}
