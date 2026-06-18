using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class TilemapDrawer : MonoBehaviour
{
    private enum NeighbourType
    {
        Air,
        Ground,
        Liquid
    }

    [Header("Tilemaps")]
    [FormerlySerializedAs("mainTilemap")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap hazardTilemap;
    [SerializeField] private Tilemap decorationTilemap;

    [Header("Biome")]
    [SerializeField] private BiomeTileSet currentBiome;

    [Header("Shared water top tiles")]
    [SerializeField] private TileBase[] waterTopLeftTiles;
    [SerializeField] private TileBase[] waterTopMiddleTiles;
    [SerializeField] private TileBase[] waterTopRightTiles;
    [SerializeField] private TileBase[] waterTopSingleTiles;

    [Header("Shared water middle tiles")]
    [SerializeField] private TileBase[] waterMiddleTiles;

    [Header("Shared water bottom tiles")]
    [SerializeField] private TileBase[] waterBottomLeftTiles;
    [SerializeField] private TileBase[] waterBottomMiddleTiles;
    [SerializeField] private TileBase[] waterBottomRightTiles;
    [SerializeField] private TileBase[] waterBottomSingleTiles;

    [Header("Shared lava top tiles")]
    [SerializeField] private TileBase[] lavaTopLeftTiles;
    [SerializeField] private TileBase[] lavaTopMiddleTiles;
    [SerializeField] private TileBase[] lavaTopRightTiles;
    [SerializeField] private TileBase[] lavaTopSingleTiles;

    [Header("Shared lava middle tiles")]
    [SerializeField] private TileBase[] lavaMiddleTiles;

    [Header("Shared lava bottom tiles")]
    [SerializeField] private TileBase[] lavaBottomLeftTiles;
    [SerializeField] private TileBase[] lavaBottomMiddleTiles;
    [SerializeField] private TileBase[] lavaBottomRightTiles;
    [SerializeField] private TileBase[] lavaBottomSingleTiles;

    [Header("Shared spike tiles")]
    [SerializeField] private TileBase[] spikeTiles;

    [Header("Air gap hazards")]
    [SerializeField] private bool drawSpikesInGaps = true;
    [SerializeField] private int spikeY = 0;

    [Header("Decorations")]
    [SerializeField] private bool drawDecorations = true;
    [Range(0, 100)]
    [SerializeField] private int smallDecorationChancePercent = 8;
    [Range(0, 100)]
    [SerializeField] private int tallDecorationChancePercent = 3;
    [SerializeField] private int minDecorationDistanceFromEdge = 1;

    [Header("Spawn area")]
    [SerializeField] private bool drawSpawnArea = true;
    [SerializeField] private int spawnStartX = -15;
    [SerializeField] private int spawnEndX = -1;
    [SerializeField] private int spawnGroundHeight = 1;
    [SerializeField] private int spawnYOffset = -2;
    [SerializeField] private int leftWallX = -16;
    [SerializeField] private int leftWallHeight = 10;

    // Stara metoda - dalej działa dla jednego chunka.
    // Czyści mapę, rysuje spawn i rysuje chunk od x = 0.
    public void DrawColumns(LevelColumnData[] columns)
    {
        if (!CanDraw(columns, currentBiome))
        {
            return;
        }

        ClearTilemap();

        if (drawSpawnArea)
        {
            DrawSpawnArea(currentBiome);
        }

        DrawColumns(columns, 0, currentBiome);

        Debug.Log("TilemapDrawer: narysowano pojedynczy chunk.");
    }

    // Nowa metoda pod ChunkManager.
    // NIE czyści tilemapy, tylko dorysowuje chunk od podanego xOffset.
    public void DrawColumns(LevelColumnData[] columns, int xOffset, BiomeTileSet biome)
    {
        if (!CanDraw(columns, biome))
        {
            return;
        }

        for (int x = 0; x < columns.Length; x++)
        {
            int worldX = x + xOffset;

            DrawBaseColumn(columns, x, worldX, biome);
            DrawFeature(columns, x, worldX, biome);
            DrawDecoration(columns, x, worldX, biome);
        }

        Debug.Log("TilemapDrawer: narysowano chunk z offsetem x = " + xOffset);
    }

    // Przyda się później dla ChunkManagera.
    public void ClearTilemap()
    {
        if (groundTilemap != null)
        {
            groundTilemap.ClearAllTiles();
        }

        if (hazardTilemap != null)
        {
            hazardTilemap.ClearAllTiles();
        }

        if (decorationTilemap != null)
        {
            decorationTilemap.ClearAllTiles();
        }
    }

    // Przyda się później dla ChunkManagera.
    public void DrawSpawn(BiomeTileSet biome)
    {
        if (groundTilemap == null)
        {
            Debug.LogError("TilemapDrawer: nie podpięto groundTilemap.");
            return;
        }

        if (biome == null)
        {
            Debug.LogError("TilemapDrawer: nie podano biome dla spawna.");
            return;
        }

        DrawSpawnArea(biome);
    }

    private bool CanDraw(LevelColumnData[] columns, BiomeTileSet biome)
    {
        if (columns == null)
        {
            Debug.LogError("TilemapDrawer dostał null columns.");
            return false;
        }

        if (groundTilemap == null)
        {
            Debug.LogError("TilemapDrawer: nie podpięto groundTilemap.");
            return false;
        }

        if (biome == null)
        {
            Debug.LogError("TilemapDrawer: nie ustawiono biome.");
            return false;
        }

        return true;
    }

    private void DrawSpawnArea(BiomeTileSet biome)
    {
        for (int x = spawnStartX; x <= spawnEndX; x++)
        {
            DrawGroundColumnWithOffset(
                x,
                spawnGroundHeight,
                spawnYOffset,
                true,
                true,
                biome
            );
        }

        for (int y = 0; y <= leftWallHeight; y++)
        {
            int finalY = y + spawnYOffset;
            Vector3Int position = new Vector3Int(leftWallX, finalY, 0);

            if (y == leftWallHeight)
            {
                groundTilemap.SetTile(position, GetRandomTile(biome.topTiles));
            }
            else
            {
                groundTilemap.SetTile(position, GetRandomTile(biome.groundTiles));
            }
        }
    }

    private void DrawBaseColumn(LevelColumnData[] columns, int localX, int worldX, BiomeTileSet biome)
    {
        LevelColumnData column = columns[localX];

        if (column.baseType == BaseColumnType.Ground)
        {
            NeighbourType leftNeighbour = GetNeighbourTypeAtHeight(columns, localX - 1, column.groundHeight);
            NeighbourType rightNeighbour = GetNeighbourTypeAtHeight(columns, localX + 1, column.groundHeight);

            bool hasSolidLeft = CountsAsSolidForGroundTop(leftNeighbour, rightNeighbour);
            bool hasSolidRight = CountsAsSolidForGroundTop(rightNeighbour, leftNeighbour);

            DrawGroundColumn(worldX, column.groundHeight, hasSolidLeft, hasSolidRight, biome);
        }
        else if (column.baseType == BaseColumnType.WaterGap)
        {
            DrawWaterColumn(columns, localX, worldX, biome);
        }
        else if (column.baseType == BaseColumnType.LavaGap)
        {
            DrawLavaColumn(columns, localX, worldX, biome);
        }
        else if (column.baseType == BaseColumnType.Gap)
        {
            DrawGapHazard(worldX, biome);
        }
    }

    private void DrawGroundColumn(
        int worldX,
        int groundHeight,
        bool hasSolidLeft,
        bool hasSolidRight,
        BiomeTileSet biome
    )
    {
        for (int y = 0; y <= groundHeight; y++)
        {
            Vector3Int position = new Vector3Int(worldX, y, 0);

            if (y == groundHeight)
            {
                TileBase topTile = ChooseTopTile(hasSolidLeft, hasSolidRight, biome);
                groundTilemap.SetTile(position, topTile);
            }
            else
            {
                groundTilemap.SetTile(position, GetRandomTile(biome.groundTiles));
            }
        }
    }

    private void DrawGroundColumnWithOffset(
        int x,
        int groundHeight,
        int yOffset,
        bool hasSolidLeft,
        bool hasSolidRight,
        BiomeTileSet biome
    )
    {
        for (int y = 0; y <= groundHeight; y++)
        {
            int finalY = y + yOffset;
            Vector3Int position = new Vector3Int(x, finalY, 0);

            if (y == groundHeight)
            {
                TileBase topTile = ChooseTopTile(hasSolidLeft, hasSolidRight, biome);
                groundTilemap.SetTile(position, topTile);
            }
            else
            {
                groundTilemap.SetTile(position, GetRandomTile(biome.groundTiles));
            }
        }
    }

    private TileBase ChooseTopTile(bool hasSolidLeft, bool hasSolidRight, BiomeTileSet biome)
    {
        bool airOnLeft = !hasSolidLeft;
        bool airOnRight = !hasSolidRight;

        if (airOnLeft && airOnRight)
        {
            return GetRandomTile(biome.topTiles);
        }

        if (airOnLeft && biome.topLeftEdgeTiles != null && biome.topLeftEdgeTiles.Length > 0)
        {
            return GetRandomTile(biome.topLeftEdgeTiles);
        }

        if (airOnRight && biome.topRightEdgeTiles != null && biome.topRightEdgeTiles.Length > 0)
        {
            return GetRandomTile(biome.topRightEdgeTiles);
        }

        return GetRandomTile(biome.topTiles);
    }

    private NeighbourType GetNeighbourTypeAtHeight(LevelColumnData[] columns, int x, int y)
    {
        if (x < 0)
        {
            x = 0;
        }
        else if (x >= columns.Length)
        {
            x = columns.Length - 1;
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
            // liquid traktujemy jak brak gruntu, żeby ground dostał TL/TR od strony cieczy.
            if (oppositeNeighbour == NeighbourType.Ground)
            {
                return false;
            }

            // Liquid + Liquid:
            // traktujemy jak solid.
            if (oppositeNeighbour == NeighbourType.Liquid)
            {
                return true;
            }
        }

        return false;
    }

    private void DrawWaterColumn(LevelColumnData[] columns, int localX, int worldX, BiomeTileSet biome)
    {
        DrawLiquidColumn(
            columns,
            localX,
            worldX,
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

    private void DrawLavaColumn(LevelColumnData[] columns, int localX, int worldX, BiomeTileSet biome)
    {
        DrawLiquidColumn(
            columns,
            localX,
            worldX,
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

    private void DrawLiquidColumn(
        LevelColumnData[] columns,
        int localX,
        int worldX,
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
        int liquidSurfaceHeight = columns[localX].groundHeight;

        bool hasSameLiquidLeft = HasSameBaseType(columns, localX - 1, liquidType);
        bool hasSameLiquidRight = HasSameBaseType(columns, localX + 1, liquidType);

        for (int y = 0; y <= liquidSurfaceHeight; y++)
        {
            Vector3Int position = new Vector3Int(worldX, y, 0);

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

            if (hazardTilemap != null)
            {
                hazardTilemap.SetTile(position, tileToDraw);
            }
        }
    }

    private void DrawGapHazard(int worldX, BiomeTileSet biome)
    {
        if (!drawSpikesInGaps || hazardTilemap == null)
        {
            return;
        }

        TileBase spikeTile = GetRandomTile(spikeTiles);

        if (spikeTile == null)
        {
            return;
        }

        Vector3Int position = new Vector3Int(worldX, spikeY, 0);
        hazardTilemap.SetTile(position, spikeTile);
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

    private void DrawFeature(LevelColumnData[] columns, int localX, int worldX, BiomeTileSet biome)
    {
        LevelColumnData column = columns[localX];

        if (column.featureType != FeatureType.Platform)
        {
            return;
        }

        bool hasPlatformLeft = HasPlatformAtHeight(columns, localX - 1, column.platformHeight);
        bool hasPlatformRight = HasPlatformAtHeight(columns, localX + 1, column.platformHeight);

        TileBase platformTile = ChooseTopTile(hasPlatformLeft, hasPlatformRight, biome);

        Vector3Int position = new Vector3Int(worldX, column.platformHeight, 0);
        groundTilemap.SetTile(position, platformTile);
    }

    private void DrawDecoration(LevelColumnData[] columns, int localX, int worldX, BiomeTileSet biome)
    {
        if (!drawDecorations || decorationTilemap == null)
        {
            return;
        }

        LevelColumnData column = columns[localX];

        if (column.baseType != BaseColumnType.Ground)
        {
            return;
        }

        if (column.featureType == FeatureType.Platform)
        {
            return;
        }

        if (IsNearGap(columns, localX, minDecorationDistanceFromEdge))
        {
            return;
        }

        TileBase decorationTile = null;

        if (Random.Range(0, 100) < tallDecorationChancePercent)
        {
            decorationTile = GetRandomTile(biome.tallDecorationTiles);
        }

        if (decorationTile == null && Random.Range(0, 100) < smallDecorationChancePercent)
        {
            decorationTile = GetRandomTile(biome.smallDecorationTiles);
        }

        if (decorationTile == null)
        {
            return;
        }

        Vector3Int position = new Vector3Int(worldX, column.groundHeight + 1, 0);
        decorationTilemap.SetTile(position, decorationTile);
    }

    private bool IsNearGap(LevelColumnData[] columns, int localX, int distance)
    {
        for (int offset = -distance; offset <= distance; offset++)
        {
            int x = localX + offset;

            if (x < 0 || x >= columns.Length)
            {
                continue;
            }

            if (columns[x].baseType != BaseColumnType.Ground)
            {
                return true;
            }
        }

        return false;
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

    private bool HasSameBaseType(LevelColumnData[] columns, int x, BaseColumnType type)
    {
        if (x < 0)
        {
            x = 0;
        }
        else if (x >= columns.Length)
        {
            x = columns.Length - 1;
        }

        return columns[x].baseType == type;
    }

    private TileBase GetRandomTile(TileBase[] tiles)
    {
        if (tiles == null || tiles.Length == 0)
        {
            return null;
        }

        return tiles[Random.Range(0, tiles.Length)];
    }

    public void ClearChunk(int xOffset, int width)
    {
        if (groundTilemap == null)
        {
            Debug.LogError("TilemapDrawer: nie podpięto groundTilemap.");
            return;
        }

        BoundsInt bounds = groundTilemap.cellBounds;

        if (hazardTilemap != null)
        {
            bounds = EncapsulateBounds(bounds, hazardTilemap.cellBounds);
        }

        if (decorationTilemap != null)
        {
            bounds = EncapsulateBounds(bounds, decorationTilemap.cellBounds);
        }

        for (int x = xOffset; x < xOffset + width; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                groundTilemap.SetTile(position, null);

                if (hazardTilemap != null)
                {
                    hazardTilemap.SetTile(position, null);
                }

                if (decorationTilemap != null)
                {
                    decorationTilemap.SetTile(position, null);
                }
            }
        }

        Debug.Log("TilemapDrawer: usunięto chunk z offsetem x = " + xOffset);
    }

    private BoundsInt EncapsulateBounds(BoundsInt first, BoundsInt second)
    {
        int minX = Mathf.Min(first.xMin, second.xMin);
        int minY = Mathf.Min(first.yMin, second.yMin);
        int minZ = Mathf.Min(first.zMin, second.zMin);

        int maxX = Mathf.Max(first.xMax, second.xMax);
        int maxY = Mathf.Max(first.yMax, second.yMax);
        int maxZ = Mathf.Max(first.zMax, second.zMax);

        return new BoundsInt(
            minX,
            minY,
            minZ,
            maxX - minX,
            maxY - minY,
            maxZ - minZ
        );
    }
}
