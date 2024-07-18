
using LargeOpenWorld.Enums;
using UdonSharp;
using UnityEngine;
using Varneon.VUdon.ArrayExtensions;
using VRC.SDKBase;

namespace LargeOpenWorld
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class IslandTileLoader : UdonSharpBehaviour
  {
    public float TileSize = 1000;
    public GameObject[] Tiles;
    public Vector2 CurrentTile { get; private set; } = new Vector2(0, 0);

    public void ChangeTile(Vector2 direction)
    {
      if (CurrentTile + direction == CurrentTile) return;
  
      CurrentTile += direction;
      
      GameObject[] nextTiles = new GameObject[9];
      Vector2[] coords = WorldTileCoordinates.GetCoordArray();

      for (int tileIndex = 0; tileIndex < coords.Length; tileIndex += 1)
      {
        nextTiles[tileIndex] = MoveLoadedTileOrLoad(coords[tileIndex], gameObject);
      }
  
      Cleanup(nextTiles);
    }

    private GameObject FindTileInLoaded(Vector2 searchTile)
    {
      for (int child = 0; child < transform.childCount; child += 1)
      {
        GameObject tile = transform.GetChild(child).gameObject;

        if (tile != null && tile.name.Contains($"({searchTile.x},{searchTile.y})"))
          return tile;
      }

      return null;
    }

    private GameObject LoadTile(Vector2 worldPosition, GameObject parent)
    {
      Vector2 tileCoordsToLoad = CurrentTile + worldPosition;
      foreach (GameObject worldTile in Tiles)
      {
        if (worldTile.name.EndsWith($"({tileCoordsToLoad.x},{tileCoordsToLoad.y})"))
        {
          GameObject instance = Instantiate(
              worldTile,
              parent.transform.parent.TransformPoint(new Vector3(worldPosition.x * 1000 - 500, 0, worldPosition.y * 1000 - 500)),
              parent.transform.parent.rotation
          );
          instance.transform.SetParent(parent.transform);

          return instance;
        }
      }

      return null;
    }

    private GameObject MoveLoadedTileOrLoad(Vector2 worldPosition, GameObject parent)
    {
      GameObject tile = FindTileInLoaded(CurrentTile + worldPosition);

      if (tile != null)
      {
        tile.transform.localPosition = new Vector3(worldPosition.x * 1000 - 500, 0, worldPosition.y * 1000 - 500);
        return tile;
      }

      return LoadTile(worldPosition, parent);
    }

    private void Cleanup(GameObject[] next)
    {
      for (int child = 0; child < transform.childCount; child += 1)
      {
        GameObject tile = transform.GetChild(child).gameObject;

        if (next.IndexOf(tile) < 0)
        {
          Destroy(tile);
        }
      }
    }
  }
}
