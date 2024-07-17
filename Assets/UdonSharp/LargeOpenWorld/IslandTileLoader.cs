using System;
using UdonSharp;
using UnityEngine;
using Varneon.VUdon.ArrayExtensions;
using LargeOpenWorld.Utils;

namespace LargeOpenWorld
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class IslandTileLoader : UdonSharpBehaviour
  {
    public float TileSize = 1000;
    public GameObject[] Tiles;
    public int ViewDistance = 1;
    public int StepMultiplier = 8;

    private Vector2Int currentTile = Vector2Int.zero;
    private int currentViewDistance = 1;

    private int[] renderTilesToLoad = new int[0];
    private GameObject[] tilesToDestroy = new GameObject[0];
    private Vector2Int[] renderDistanceCoords = new Vector2Int[0];

    void LateUpdate()
    {
      // Progressively load and unload tiles each update.

      if (renderTilesToLoad.Length == 1)
      {
        LoadTile(renderTilesToLoad[0], gameObject);
        renderTilesToLoad = renderTilesToLoad.RemoveAt(0);
      }
      else if (renderTilesToLoad.Length > 0)
      {
        foreach (int tileCoordsIndex in renderTilesToLoad.GetRange(0, 2))
        {
          LoadTile(tileCoordsIndex, gameObject);
        }

        renderTilesToLoad = renderTilesToLoad.RemoveRange(0, 2);
      }

      if (tilesToDestroy.Length == 1)
      {
        Destroy(tilesToDestroy[0]);
        tilesToDestroy = tilesToDestroy.RemoveAt(0);
      }
      else if (tilesToDestroy.Length > 0)
      {
        foreach (GameObject tile in tilesToDestroy.GetRange(0, 2))
        {
          Destroy(tile);
        }

        tilesToDestroy = tilesToDestroy.RemoveRange(0, 2);
      }

      if (ViewDistance != currentViewDistance)
      {
        currentViewDistance = ViewDistance;
        GenerateCoordsForRenderDistance();
        MoveInDirection(Vector2Int.zero);
      }
    }

    /// <summary>
    /// Move world in a direction.
    /// </summary>
    /// <param name="direction"></param>
    public void MoveInDirection(Vector2Int direction)
    {
      if (renderDistanceCoords.Length == 0)
      {
        Debug.Log("Coords not calculated");
        GenerateCoordsForRenderDistance();
      }

      MoveLoadedTiles(direction);
      // I can figure out which tiles need to be loaded by taking the direction of movement and using the view distance to calculate the last row in matrix to load.
      // Additional logic is required for diagonals. Will need to translate tiles currently marked as needed to load and delete ones that are out of range.
      currentTile += direction;
    }

    public Vector2Int GetLatestTile()
    {
      return currentTile;
    }

    /// <summary>
    /// Load world tile for render coordinate index.
    /// </summary>
    /// <param name="coordsIndex"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private void LoadTile(int coordsIndex, GameObject parent)
    {
      Vector2Int renderCoords = renderDistanceCoords[coordsIndex];
      string worldCoords = $"({(currentTile + renderCoords).x},{(currentTile + renderCoords).y})";
      float halfTileSize = TileSize / 2;

      foreach (GameObject worldTile in Tiles)
      {
        if (worldTile.name.EndsWith(worldCoords))
        {
          GameObject instance = Instantiate(
            worldTile,
            parent.transform.parent.TransformPoint(new Vector3(renderCoords.x * TileSize - halfTileSize, 0, renderCoords.y * TileSize - halfTileSize)),
            parent.transform.parent.rotation
          );
          instance.transform.SetParent(parent.transform);
        }
      }
    }

    /// <summary>
    /// Move currently loaded tiles in direction of travel and calculate tiles to destroy.
    /// </summary>
    /// <param name="direction">Direction of travel</param>
    private void MoveLoadedTiles(Vector2Int direction)
    {
      Vector3 directionVector = new Vector3(direction.x * TileSize, 0, direction.y * TileSize);

      for (int childIndex = 0; childIndex < gameObject.transform.childCount; childIndex += 1)
      {
        Transform tile = gameObject.transform.GetChild(childIndex);
        tile.localPosition += directionVector;

        if (Vector3.Distance(tile.localPosition, Vector3.zero) > ViewDistance * TileSize)
        {
          tilesToDestroy = tilesToDestroy.Add(tile.gameObject);
        }
      }
    }

    private void GenerateCoordsForRenderDistance()
    {
      int algViewDistance = currentViewDistance * 2 + 1;
      int halfViewDistance = algViewDistance / 2 + 1;
      renderDistanceCoords = new Vector2Int[(int)Math.Pow(algViewDistance, 2)];
      Vector2Int tile = Vector2Int.zero;
      Vector2Int iterationDirection = Vector2Int.down;

      for (int tileIndex = 0; tileIndex < renderDistanceCoords.Length; tileIndex += 1)
      {
        if (
          -halfViewDistance < tile.x && tile.x <= halfViewDistance
          && -halfViewDistance < tile.y && tile.y <= halfViewDistance
        )
        {
          renderDistanceCoords[tileIndex].x = tile.x;
          renderDistanceCoords[tileIndex].y = tile.y;
        }

        if (
          tile.x == tile.y
          || (tile.x == -tile.y && tile.x < 0)
          || (tile.x == 1 - tile.y && tile.x > 0)
        )
        {
          iterationDirection.x += iterationDirection.y;
          iterationDirection.y = iterationDirection.x - iterationDirection.y;
          iterationDirection.x = -(iterationDirection.x - iterationDirection.y);
        }

        tile += iterationDirection;
      }
    }
  }
}
