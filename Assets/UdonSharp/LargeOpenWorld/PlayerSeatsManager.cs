using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace LargeOpenWorld
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
  public class PlayerSeatsManager : UdonSharpBehaviour
  {
    public PlayerSeat[] PlayerSeats;
    public Island Island;

    [System.NonSerialized]
    public PlayerSeat LocalSeat;

    private int seatsReady = 0;

    void Start()
    {
      if (Networking.LocalPlayer.isMaster)
      {
        AssignFreeSeat();
      }
    }

    public void ClearLocalSeat()
    {
      LocalSeat = null;
    }

    public void SeatReady()
    {
      seatsReady += 1;

      if (seatsReady == PlayerSeats.Length)
      {
        AssignFreeSeat();
      }
    }

    public void ProcessPlayerChangedTile(Vector2 tile)
    {
      foreach (PlayerSeat seat in PlayerSeats)
      {
        if (seat.IsOwnedByLocal)
        {
          seat.DispatchTileChange(tile);
        }
        else if (seat.Owner != -1)
        {
          // Updated position early to prevent flicker in remote user position when changing tile. 
          seat.UpdatePositionWithNetworkData(true);
        }
      }
    }

    private void AssignFreeSeat()
    {
      if (!LocalSeat)
      {
        PlayerSeat seat = GetFreeSeat();
        LocalSeat = seat;

        if (Networking.IsOwner(seat.gameObject))
        {
          LocalSeat = seat;
          seat.FinaliseOwnership();
        }
        else
        {
          Networking.SetOwner(Networking.LocalPlayer, seat.gameObject);
        }
      }
    }

    private PlayerSeat GetFreeSeat()
    {
      foreach (PlayerSeat seat in PlayerSeats)
      {
        if (seat.Owner == -1)
        {
          return seat;
        }
      }

      return null;
    }
  }
}
