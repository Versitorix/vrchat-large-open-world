using UnityEngine;

namespace LargeOpenWorld.Utils
{
  static class ArrayUtils
  {
    public static int FindVector2IndexInArray(Vector2Int[] array, Vector2Int vector)
    {
      for (int index = 0; index < array.Length; index += 1)
      {
        if (array[index].x == vector.x && array[index].y == vector.y)
        {
          return index;
        }
      }

      return -1;
    }
  }
}