using System.Linq;
using LargeOpenWorld.Enums;
using LargeOpenWorld.Editor.Utils;
using UnityEditor;
using UnityEngine;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace LargeOpenWorld.Editor
{

  [CustomEditor(typeof(WorldPackager))]
  public class WorldPackagerEditor : UnityEditor.Editor
  {
    public override void OnInspectorGUI()
    {
      WorldPackager worldEditor = (WorldPackager)target;

      worldEditor.BaseName = EditorGUILayout.TextField("Base name", worldEditor.BaseName);
      worldEditor.TileSize = EditorGUILayout.FloatField("Tile size", worldEditor.TileSize);
      worldEditor.MapDimensions = EditorGUILayout.Vector2Field("Map dimensions", worldEditor.MapDimensions);
      float width = worldEditor.MapDimensions.x * worldEditor.TileSize;
      float height = worldEditor.MapDimensions.y * worldEditor.TileSize;
      float area = height / 1000 * (width / 1000);
      EditorGUILayout.LabelField($"Map area: {width}m x {height}m ({area}km²)");
      EditorGUILayout.Separator();

      if (GUILayout.Button("Generate Tiles"))
      {
        GenerateTilePrefabs();
      }

      if (GUILayout.Button("Generate World Prefab"))
      {
        GenerateMapPrefab();
      }

      if (GUILayout.Button("Generate All"))
      {
        GenerateTilePrefabs();
        GenerateMapPrefab();
      }

    }

    private GameObject[] GetTiles(WorldPackager worldPackager)
    {
      GameObject[] tiles = new GameObject[worldPackager.transform.childCount];

      for (int childIndex = 0; childIndex < worldPackager.transform.childCount; childIndex += 1)
      {
        tiles[childIndex] = worldPackager.transform.GetChild(childIndex).gameObject;
      }

      return tiles;
    }

    private void GenerateTilePrefabs()
    {
      WorldPackager worldPackager = (WorldPackager)target;
      GameObject[] worldTiles = GetTiles(worldPackager);
      GameObject origin = worldTiles.First((tile) => tile.name == worldPackager.BaseName);

      foreach (GameObject tile in worldTiles)
      {
        if (PrefabUtility.IsAnyPrefabInstanceRoot(tile))
        {
          PrefabUtility.UnpackPrefabInstance(tile, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
      }

      MapFileUtils.CleanTilePerfabsFolder(worldPackager.BaseName);

      foreach (GameObject tile in worldTiles)
      {
        MapFileUtils.SaveTilePrefab(
          tile,
          worldPackager.BaseName,
          origin.transform.localPosition,
          worldPackager.TileSize,
          out bool prefabSuccess
        );

        if (prefabSuccess == false)
          throw new System.Exception($"Failed to save a tile!: Tile \"{tile.name}\" causes errors");
      }
    }

    private void GenerateMapPrefab()
    {
      WorldPackager worldPackager = (WorldPackager)target;

      MapFileUtils.CleanMapPrefab(worldPackager.BaseName);
      GameObject[] worldTilePrefabs = MapFileUtils.GetTileAssets(worldPackager.BaseName);

      GameObject worldPrefab = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Island.prefab")) as GameObject;
      IslandTileLoader tileLoader = worldPrefab.GetComponentInChildren<IslandTileLoader>();
      tileLoader.Tiles = worldTilePrefabs;
      tileLoader.TileSize = worldPackager.TileSize;
      Transform geometry = worldPrefab.transform.GetChild(0);
      WorldTileCoordinates.GetCoordArray().ForEach((coord) =>
      {
        GameObject tilePrefab = worldTilePrefabs.First((gameObject) => gameObject.name.EndsWith($"({coord.x},{coord.y})"));
        GameObject tileInstance = PrefabUtility.InstantiatePrefab(tilePrefab, geometry) as GameObject;
        tileInstance.transform.localPosition = new Vector3(coord.x * worldPackager.TileSize - (worldPackager.TileSize / 2), 0, coord.y * worldPackager.TileSize - (worldPackager.TileSize / 2));
      });

      MapFileUtils.SaveMapPrefab(worldPrefab, worldPackager.BaseName);
      DestroyImmediate(worldPrefab);
    }
  }
}
