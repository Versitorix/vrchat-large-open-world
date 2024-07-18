using UnityEngine;

public class WorldPackager : MonoBehaviour
{
    public string BaseName { get; set; } = "Terrain";
    public float TileSize { get; set; } = 1000f;
    public Vector2 MapDimensions { get; set; } = Vector2.one;
}
