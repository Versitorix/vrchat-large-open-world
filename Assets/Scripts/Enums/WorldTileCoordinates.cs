using UnityEngine;

namespace LargeOpenWorld.Enums
{
  public static class WorldTileCoordinates
  {
    public static Vector2[] GetCoordArray()
    {
      Vector2[] coords = new Vector2[9];
      coords[0] = Vector2.zero;
      coords[1] = Vector2.up;
      coords[2] = Vector2.down;
      coords[3] = Vector2.left;
      coords[4] = Vector2.right;
      coords[5] = Vector2.up + Vector2.left;
      coords[6] = Vector2.up + Vector2.right;
      coords[7] = Vector2.down + Vector2.left;
      coords[8] = Vector2.down + Vector2.right;

      return coords;
    }
  }
}