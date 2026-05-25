using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapDrawer : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap mainTilemap;

    [Header("Ground tiles")]
    [SerializeField] private TileBase[] groundTiles;

    [Header("Top tiles")]
    [SerializeField] private TileBase[] topTiles;
    [SerializeField] private TileBase[] topLeftEdgeTiles;
    [SerializeField] private TileBase[] topRightEdgeTiles;
    [SerializeField] private TileBase[] topSingleTiles;

    [Header("Water top tiles")]
    [SerializeField] private TileBase[] waterTopLeftTiles;
    [SerializeField] private TileBase[] waterTopMiddleTiles;
    [SerializeField] private TileBase[] waterTopRightTiles;
    [SerializeField] private TileBase[] waterTopSingleTiles;

    [Header("Water middle tiles")]
    [SerializeField] private TileBase[] waterMiddleTiles;

    [Header("Water bottom tiles")]
    [SerializeField] private TileBase[] waterBottomLeftTiles;
    [SerializeField] private TileBase[] waterBottomMiddleTiles;
    [SerializeField] private TileBase[] waterBottomRightTiles;
    [SerializeField] private TileBase[] waterBottomSingleTiles;

    [Header("Lava top tiles")]
    [SerializeField] private TileBase[] lavaTopLeftTiles;
    [SerializeField] private TileBase[] lavaTopMiddleTiles;
    [SerializeField] private TileBase[] lavaTopRightTiles;
    [SerializeField] private TileBase[] lavaTopSingleTiles;

    [Header("Lava middle tiles")]
    [SerializeField] private TileBase[] lavaMiddleTiles;

    [Header("Lava bottom tiles")]
    [SerializeField] private TileBase[] lavaBottomLeftTiles;
    [SerializeField] private TileBase[] lavaBottomMiddleTiles;
    [SerializeField] private TileBase[] lavaBottomRightTiles;
    [SerializeField] private TileBase[] lavaBottomSingleTiles;

    [Header("Spawn area")]
    [SerializeField] private bool drawSpawnArea = true;
    [SerializeField] private int spawnStartX = -15;
    [SerializeField] private int spawnEndX = -1;
    [SerializeField] private int spawnGroundHeight = 1;
    [SerializeField] private int spawnYOffset = -2;
    [SerializeField] private int leftWallX = -16;
    [SerializeField] private int leftWallHeight = 10;

    private enum NeighbourType
    {
        Air,
        Ground,
        Liquid
    }

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
            DrawSpawnArea(columns);
        }

        for (int x = 0; x < columns.Length; x++)
        {
            DrawBaseColumn(columns, x);
            DrawFeature(columns, x);
        }

        Debug.Log("TilemapDrawer: narysowano chunk.");
    }

    private void DrawSpawnArea(LevelColumnData[] columns)
    {
        for (int x = spawnStartX; x <= spawnEndX; x++)
        {
            DrawGroundColumnWithOffset(x, spawnGroundHeight, spawnYOffset, false, false);
        }

        for (int y = 0; y <= leftWallHeight; y++)
        {
            int finalY = y + spawnYOffset;
            Vector3Int position = new Vector3Int(leftWallX, finalY, 0);

            if (y == leftWallHeight)
            {
                mainTilemap.SetTile(position, GetRandomTile(topTiles));
            }
            else
            {
                mainTilemap.SetTile(position, GetRandomTile(groundTiles));
            }
        }
    }

    private void DrawGroundColumnWithOffset(
        int x,
        int groundHeight,
        int yOffset,
        bool hasGroundLeft,
        bool hasGroundRight
    )
    {
        for (int y = 0; y <= groundHeight; y++)
        {
            int finalY = y + yOffset;
            Vector3Int position = new Vector3Int(x, finalY, 0);

            if (y == groundHeight)
            {
                TileBase topTile = ChooseTopTile(hasGroundLeft, hasGroundRight);
                mainTilemap.SetTile(position, topTile);
            }
            else
            {
                mainTilemap.SetTile(position, GetRandomTile(groundTiles));
            }
        }
    }

    private void DrawBaseColumn(LevelColumnData[] columns, int x)
    {
        LevelColumnData column = columns[x];

        if (column.baseType == BaseColumnType.Ground)
        {
            NeighbourType leftNeighbour = GetNeighbourTypeAtHeight(columns, x - 1, column.groundHeight);
            NeighbourType rightNeighbour = GetNeighbourTypeAtHeight(columns, x + 1, column.groundHeight);

            bool hasSolidLeft = CountsAsSolidForGroundTop(leftNeighbour, rightNeighbour);
            bool hasSolidRight = CountsAsSolidForGroundTop(rightNeighbour, leftNeighbour);

            DrawGroundColumn(x, column.groundHeight, hasSolidLeft, hasSolidRight);
        }
        else if (column.baseType == BaseColumnType.WaterGap)
        {
            DrawWaterColumn(columns, x);
        }
        else if (column.baseType == BaseColumnType.LavaGap)
        {
            DrawLavaColumn(columns, x);
        }
        else if (column.baseType == BaseColumnType.Gap)
        {
            // Zwykła przepaść - nic nie rysujemy.
        }
    }
    private void DrawWaterColumn(LevelColumnData[] columns, int x)
    {
        DrawLiquidColumn(
            columns,
            x,
            BaseColumnType.WaterGap,
            waterTopLeftTiles,
            waterTopMiddleTiles,
            waterTopRightTiles,
            waterTopSingleTiles,
            waterMiddleTiles,
            waterBottomLeftTiles,
            waterBottomMiddleTiles,
            waterBottomRightTiles,
            waterBottomSingleTiles
        );
    }

    private void DrawLavaColumn(LevelColumnData[] columns, int x)
    {
        DrawLiquidColumn(
            columns,
            x,
            BaseColumnType.LavaGap,
            lavaTopLeftTiles,
            lavaTopMiddleTiles,
            lavaTopRightTiles,
            lavaTopSingleTiles,
            lavaMiddleTiles,
            lavaBottomLeftTiles,
            lavaBottomMiddleTiles,
            lavaBottomRightTiles,
            lavaBottomSingleTiles
        );
    }

    private void DrawGroundColumn(int x, int groundHeight, bool hasGroundLeft, bool hasGroundRight)
    {
        for (int y = 0; y <= groundHeight; y++)
        {
            Vector3Int position = new Vector3Int(x, y, 0);

            if (y == groundHeight)
            {
                TileBase topTile = ChooseTopTile(hasGroundLeft, hasGroundRight);
                mainTilemap.SetTile(position, topTile);
            }
            else
            {
                mainTilemap.SetTile(position, GetRandomTile(groundTiles));
            }
        }
    }

    private TileBase ChooseTopTile(bool hasSolidLeft, bool hasSolidRight)
    {
        bool airOnLeft = !hasSolidLeft;
        bool airOnRight = !hasSolidRight;

        // Jeśli po obu stronach jest powietrze, dajemy zwykły top.
        // Czyli nie robimy isolated tile, bo tak chcesz wizualnie.
        if (airOnLeft && airOnRight)
        {
            return GetRandomTile(topTiles);
        }

        // Jeśli po lewej jest powietrze, to lewa krawędź.
        if (airOnLeft && topLeftEdgeTiles.Length > 0)
        {
            return GetRandomTile(topLeftEdgeTiles);
        }

        // Jeśli po prawej jest powietrze, to prawa krawędź.
        if (airOnRight && topRightEdgeTiles.Length > 0)
        {
            return GetRandomTile(topRightEdgeTiles);
        }

        // Normalny środek powierzchni.
        return GetRandomTile(topTiles);
    }
    private bool HasPlatformAtHeight(LevelColumnData[] columns, int x, int y)
    {
        if (x < 0 || x >= columns.Length)
        {
            return false;
        }

        LevelColumnData column = columns[x];

        return column.featureType == FeatureType.Platform &&
               column.platformHeight == y;
    }

    private bool HasSolidAtHeight(LevelColumnData[] columns, int x, int y)
    {
        if (x < 0 || x >= columns.Length)
        {
            return false;
        }

        LevelColumnData column = columns[x];

        // Normalny ground jest solidem od y=0 do groundHeight.
        if (column.baseType == BaseColumnType.Ground)
        {
            return column.groundHeight >= y;
        }

        // Woda/lawa też liczą się jako "wypełniona" kolumna
        // do swojej wysokości, żeby ground obok nie dostawał edge'a.
        if (column.baseType == BaseColumnType.WaterGap ||
            column.baseType == BaseColumnType.LavaGap)
        {
            return column.groundHeight >= y;
        }

        // Zwykły Gap to powietrze.
        return false;
    }
    private bool HasGroundNeighbour(LevelColumnData[] columns, int x)
    {
        if (x < 0 || x >= columns.Length)
        {
            return false;
        }

        return columns[x].baseType == BaseColumnType.Ground;
    }

    private void DrawLiquidColumn(
     LevelColumnData[] columns,
     int x,
     BaseColumnType liquidType,

     TileBase[] topLeftTiles,
     TileBase[] topMiddleTiles,
     TileBase[] topRightTiles,
     TileBase[] topSingleTiles,

     TileBase[] middleTiles,

     TileBase[] bottomLeftTiles,
     TileBase[] bottomMiddleTiles,
     TileBase[] bottomRightTiles,
     TileBase[] bottomSingleTiles
 )
    {
        int liquidSurfaceHeight = columns[x].groundHeight;

        bool hasSameLiquidLeft = HasSameBaseType(columns, x - 1, liquidType);
        bool hasSameLiquidRight = HasSameBaseType(columns, x + 1, liquidType);

        for (int y = 0; y <= liquidSurfaceHeight; y++)
        {
            Vector3Int position = new Vector3Int(x, y, 0);

            TileBase tileToDraw;

            if (y == liquidSurfaceHeight)
            {
                tileToDraw = ChooseHorizontalTile(
                    hasSameLiquidLeft,
                    hasSameLiquidRight,
                    topLeftTiles,
                    topMiddleTiles,
                    topRightTiles,
                    topSingleTiles
                );
            }
            else if (y == 0)
            {
                tileToDraw = ChooseHorizontalTile(
                    hasSameLiquidLeft,
                    hasSameLiquidRight,
                    bottomLeftTiles,
                    bottomMiddleTiles,
                    bottomRightTiles,
                    bottomSingleTiles
                );
            }
            else
            {
                tileToDraw = GetRandomTile(middleTiles);
            }

            mainTilemap.SetTile(position, tileToDraw);
        }
    }

    private TileBase ChooseHorizontalTile(
    bool hasSameLeft,
    bool hasSameRight,
    TileBase[] leftTiles,
    TileBase[] middleTiles,
    TileBase[] rightTiles,
    TileBase[] singleTiles
)
    {
        if (!hasSameLeft && !hasSameRight && singleTiles != null && singleTiles.Length > 0)
        {
            return GetRandomTile(singleTiles);
        }

        if (!hasSameLeft && leftTiles != null && leftTiles.Length > 0)
        {
            return GetRandomTile(leftTiles);
        }

        if (!hasSameRight && rightTiles != null && rightTiles.Length > 0)
        {
            return GetRandomTile(rightTiles);
        }

        return GetRandomTile(middleTiles);
    }

    private void DrawFeature(LevelColumnData[] columns, int x)
    {
        LevelColumnData column = columns[x];

        if (column.featureType == FeatureType.Platform)
        {
            bool hasPlatformLeft = HasPlatformAtHeight(columns, x - 1, column.platformHeight);
            bool hasPlatformRight = HasPlatformAtHeight(columns, x + 1, column.platformHeight);

            TileBase platformTile = ChooseTopTile(hasPlatformLeft, hasPlatformRight);

            Vector3Int position = new Vector3Int(x, column.platformHeight, 0);
            mainTilemap.SetTile(position, platformTile);
        }
    }

    private bool HasSameBaseType(LevelColumnData[] columns, int x, BaseColumnType type)
    {
        if (x < 0 || x >= columns.Length)
        {
            return false;
        }

        return columns[x].baseType == type;
    }

    private NeighbourType GetNeighbourTypeAtHeight(LevelColumnData[] columns, int x, int y)
    {
        if (x < 0 || x >= columns.Length)
        {
            return NeighbourType.Air;
        }

        LevelColumnData column = columns[x];

        if (column.baseType == BaseColumnType.Ground && column.groundHeight >= y)
        {
            return NeighbourType.Ground;
        }

        if ((column.baseType == BaseColumnType.WaterGap ||
             column.baseType == BaseColumnType.LavaGap) &&
            column.groundHeight >= y)
        {
            return NeighbourType.Liquid;
        }

        return NeighbourType.Air;
    }

    private bool CountsAsSolidForGroundTop(NeighbourType neighbour, NeighbourType oppositeNeighbour)
    {
        if (neighbour == NeighbourType.Ground)
        {
            return true;
        }

        if (neighbour == NeighbourType.Air)
        {
            return false;
        }

        if (neighbour == NeighbourType.Liquid)
        {
            // Air + Liquid:
            // liquid traktujemy jak solid, żeby edge był tylko od strony powietrza.
            if (oppositeNeighbour == NeighbourType.Air)
            {
                return true;
            }

            // Liquid + Ground:
            // liquid traktujemy jak brak ziemi, żeby ground dostał TL/TR od strony cieczy.
            if (oppositeNeighbour == NeighbourType.Ground)
            {
                return false;
            }

            // Liquid + Liquid:
            // traktujemy jak solid, żeby samotny ground w cieczy nie dostał dziwnych edge'ów z obu stron.
            if (oppositeNeighbour == NeighbourType.Liquid)
            {
                return true;
            }
        }

        return false;
    }
    private TileBase GetRandomTile(TileBase[] tiles)
    {
        if (tiles == null || tiles.Length == 0)
        {
            return null;
        }

        return tiles[Random.Range(0, tiles.Length)];
    }
}