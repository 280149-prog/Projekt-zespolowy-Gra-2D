using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class BiomeTileSet
{
    [Header("Biome info")]
    public string biomeName;

    [Header("Ground tiles")]
    public TileBase[] groundTiles;

    [Header("Top tiles")]
    public TileBase[] topTiles;
    public TileBase[] topLeftEdgeTiles;
    public TileBase[] topRightEdgeTiles;
    public TileBase[] topSingleTiles;
}