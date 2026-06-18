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

    [Header("Decoration tiles")]
    public TileBase[] smallDecorationTiles;
    public TileBase[] tallDecorationTiles;

    [Header("Decoration prefabs")]
    public GameObject[] wideDecorationPrefabs;
    public GameObject[] tallDecorationPrefabs;
    public GameObject[] largeDecorationPrefabs;
}
