using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace LargeOpenWorld.Editor.Utils
{
  public static class MapFileUtils
  {
    private static string BaseMapFolder { get => "Assets/Prefabs/Maps"; }
  
    public static void CleanTilePerfabsFolder(string mapName)
    {
      if (!Directory.Exists("Assets/Prefabs"))
        AssetDatabase.CreateFolder("Assets", "Prefabs");

      if (!Directory.Exists(BaseMapFolder))
        AssetDatabase.CreateFolder("Assets/Prefabs", "Maps");

      if (!Directory.Exists($"{BaseMapFolder}/{mapName}"))
      {
        AssetDatabase.CreateFolder(BaseMapFolder, mapName);
      }
      else
      {
        Directory.GetFiles($"{BaseMapFolder}/{mapName}")
          .ForEach((file) =>
          {
            File.Delete(file);
          });
      }
    }

    public static void CleanMapPrefab(string mapName)
    {
      if (File.Exists($"{BaseMapFolder}/{mapName}-Map.prefab"))
      {
        File.Delete($"{BaseMapFolder}/{mapName}-Map.prefab");
        File.Delete($"{BaseMapFolder}/{mapName}-Map.meta");
      }
    }

    public static GameObject[] GetTileAssets(string mapName)
    {
      string[] tileFiles = Directory.GetFiles($"{BaseMapFolder}/{mapName}/").Where((file) => file.EndsWith(".prefab")).ToArray();
      GameObject[] tileAssets = new GameObject[tileFiles.Length];

      for (int tileIndex = 0; tileIndex < tileFiles.Length; tileIndex += 1)
      {
        tileAssets[tileIndex] = AssetDatabase.LoadAssetAtPath<GameObject>(tileFiles[tileIndex]);
      }

      return tileAssets;
    }

    public static GameObject SaveTilePrefab(GameObject tile, string baseName, Vector3 baseTileLocation, float tileSize)
    {
      return SaveTilePrefab(tile, baseName, baseTileLocation, tileSize, out bool _);
    }
  
    public static GameObject SaveTilePrefab(GameObject tile, string baseName, Vector3 baseTileLocation, float tileSize, out bool prefabSuccess)
    {
      float x = (tile.transform.localPosition.x - baseTileLocation.x) / tileSize;
      float y = (tile.transform.localPosition.z - baseTileLocation.z) / tileSize;
      string localPath = AssetDatabase.GenerateUniqueAssetPath($"{BaseMapFolder}/{baseName}/{baseName}_({x},{y}).prefab");

      return PrefabUtility.SaveAsPrefabAssetAndConnect(tile, localPath, InteractionMode.UserAction, out prefabSuccess);
    }


    public static GameObject SaveMapPrefab(GameObject mapGameObject, string mapName)
    {
      return SaveMapPrefab(mapGameObject, mapName, out bool _);
    }
  
    public static GameObject SaveMapPrefab(GameObject mapGameObject, string mapName, out bool prefabSuccess)
    {
      string worldPath = AssetDatabase.GenerateUniqueAssetPath($"{BaseMapFolder}/{mapName}-Map.prefab");
    
      return PrefabUtility.SaveAsPrefabAsset(mapGameObject, worldPath, out prefabSuccess);
    }
  }
}