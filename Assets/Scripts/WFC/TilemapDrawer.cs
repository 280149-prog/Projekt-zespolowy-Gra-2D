using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapDrawer : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap mainTilemap;

    [Header("Ground tiles")]
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase topTile;

    [Header("Special tiles")]
    [SerializeField] private TileBase waterTile;
    [SerializeField] private TileBase lavaTile;
    [SerializeField] private TileBase platformTile;

    [Header("Spawn area")]
    [SerializeField] private bool drawSpawnArea = true;
    [SerializeField] private int spawnStartX = -15;
    [SerializeField] private int spawnEndX = -1;
    [SerializeField] private int spawnGroundHeight = 1;
    [SerializeField] private int spawnYOffset = -2;
    [SerializeField] private int leftWallX = -16;
    [SerializeField] private int leftWallHeight = 10;

    public void DrawColumns(LevelColumnData[] columns)
    {
        if (columns == null)
        {
            Debug.LogError("TilemapDrawer dostał null columns.");
            return;
        }

        mainTilemap.ClearAllTiles();

        if (drawSpawnArea)
        {
            DrawSpawnArea();
        }

        for (int x = 0; x < columns.Length; x++)
        {
            DrawBaseColumn(columns[x], x);
            DrawFeature(columns[x], x);
        }

        Debug.Log("TilemapDrawer: narysowano chunk.");
    }

    private void DrawSpawnArea()
    {
        // Podłoga spawna od spawnStartX do spawnEndX.
        for (int x = spawnStartX; x <= spawnEndX; x++)
        {
            DrawGroundColumnWithOffset(x, spawnGroundHeight, spawnYOffset);
        }

        // Lewa ściana blokująca wyjście poza mapę.
        for (int y = 0; y <= leftWallHeight; y++)
        {
            int finalY = y + spawnYOffset;

            Vector3Int position = new Vector3Int(leftWallX, finalY, 0);

            if (y == leftWallHeight)
            {
                mainTilemap.SetTile(position, topTile);
            }
            else
            {
                mainTilemap.SetTile(position, groundTile);
            }
        }
    }

    private void DrawGroundColumnWithOffset(int x, int groundHeight, int yOffset)
    {
        for (int y = 0; y <= groundHeight; y++)
        {
            int finalY = y + yOffset;

            Vector3Int position = new Vector3Int(x, finalY, 0);

            if (y == groundHeight)
            {
                mainTilemap.SetTile(position, topTile);
            }
            else
            {
                mainTilemap.SetTile(position, groundTile);
            }
        }
    }

    private void DrawBaseColumn(LevelColumnData column, int x)
    {
        if (column.baseType == BaseColumnType.Ground)
        {
            DrawGroundColumn(x, column.groundHeight);
        }
        else if (column.baseType == BaseColumnType.WaterGap)
        {
            DrawLiquidColumn(x, waterTile, column.groundHeight);
        }
        else if (column.baseType == BaseColumnType.LavaGap)
        {
            DrawLiquidColumn(x, lavaTile, column.groundHeight);
        }
        else if (column.baseType == BaseColumnType.Gap)
        {
            // Zwykła przepaść - nic nie rysujemy.
        }
    }

    private void DrawGroundColumn(int x, int groundHeight)
    {
        for (int y = 0; y <= groundHeight; y++)
        {
            Vector3Int position = new Vector3Int(x, y, 0);

            if (y == groundHeight)
            {
                mainTilemap.SetTile(position, topTile);
            }
            else
            {
                mainTilemap.SetTile(position, groundTile);
            }
        }
    }

    private void DrawLiquidColumn(int x, TileBase liquidTile, int liquidSurfaceHeight)
    {
        if (liquidTile == null)
        {
            return;
        }

        for (int y = 0; y <= liquidSurfaceHeight; y++)
        {
            Vector3Int position = new Vector3Int(x, y, 0);
            mainTilemap.SetTile(position, liquidTile);
        }
    }

    private void DrawFeature(LevelColumnData column, int x)
    {
        if (column.featureType == FeatureType.Platform)
        {
            Vector3Int position = new Vector3Int(x, column.platformHeight, 0);
            mainTilemap.SetTile(position, platformTile);
        }
    }
}