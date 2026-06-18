using System.Collections.Generic;
using UnityEngine;

public class DecorationSpawner : MonoBehaviour
{
    private enum DecorationSize
    {
        Wide,
        Tall,
        Large
    }

    [Header("Spawn chances")]
    [Range(0, 100)]
    [SerializeField] private int wideDecorationChancePercent = 4;
    [Range(0, 100)]
    [SerializeField] private int tallDecorationChancePercent = 3;
    [Range(0, 100)]
    [SerializeField] private int largeDecorationChancePercent = 2;

    [Header("Placement rules")]
    [SerializeField] private int minDistanceFromChunkEdge = 2;
    [SerializeField] private int minDistanceFromGap = 2;
    [SerializeField] private int wideAndLargeSideClearance = 2;
    [SerializeField] private int minDistanceBetweenPrefabDecorations = 5;
    [SerializeField] private bool randomFlipX = true;

    [Header("Sorting")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int orderInLayer = -1;

    private readonly Dictionary<int, Transform> chunkParents = new Dictionary<int, Transform>();

    public void SpawnDecorations(LevelColumnData[] columns, int xOffset, int chunkIndex, BiomeTileSet biome)
    {
        if (columns == null || biome == null)
        {
            return;
        }

        Transform chunkParent = GetOrCreateChunkParent(chunkIndex);
        int lastSpawnedLocalX = -999;

        for (int localX = minDistanceFromChunkEdge; localX < columns.Length - minDistanceFromChunkEdge; localX++)
        {
            if (localX - lastSpawnedLocalX < minDistanceBetweenPrefabDecorations)
            {
                continue;
            }

            if (!CanPlaceDecoration(columns, localX, minDistanceFromGap))
            {
                continue;
            }

            GameObject prefab = ChooseDecorationPrefab(columns, localX, biome, out DecorationSize size);

            if (prefab == null)
            {
                continue;
            }

            if (!HasEnoughFlatGround(columns, localX, size))
            {
                continue;
            }

            SpawnPrefab(prefab, columns[localX], xOffset, localX, chunkParent);
            lastSpawnedLocalX = localX;
        }
    }

    public void ClearDecorationsForChunk(int chunkIndex)
    {
        if (!chunkParents.TryGetValue(chunkIndex, out Transform parent))
        {
            return;
        }

        if (parent != null)
        {
            Destroy(parent.gameObject);
        }

        chunkParents.Remove(chunkIndex);
    }

    public void ClearAllDecorations()
    {
        foreach (Transform parent in chunkParents.Values)
        {
            if (parent != null)
            {
                Destroy(parent.gameObject);
            }
        }

        chunkParents.Clear();
    }

    private GameObject ChooseDecorationPrefab(
        LevelColumnData[] columns,
        int localX,
        BiomeTileSet biome,
        out DecorationSize size
    )
    {
        size = DecorationSize.Wide;

        if (Random.Range(0, 100) < largeDecorationChancePercent)
        {
            GameObject prefab = GetRandomPrefab(biome.largeDecorationPrefabs);

            if (prefab != null)
            {
                size = DecorationSize.Large;
                return prefab;
            }
        }

        if (Random.Range(0, 100) < tallDecorationChancePercent)
        {
            GameObject prefab = GetRandomPrefab(biome.tallDecorationPrefabs);

            if (prefab != null)
            {
                size = DecorationSize.Tall;
                return prefab;
            }
        }

        if (Random.Range(0, 100) < wideDecorationChancePercent)
        {
            GameObject prefab = GetRandomPrefab(biome.wideDecorationPrefabs);

            if (prefab != null)
            {
                size = DecorationSize.Wide;
                return prefab;
            }
        }

        return null;
    }

    private bool CanPlaceDecoration(LevelColumnData[] columns, int localX, int gapDistance)
    {
        LevelColumnData column = columns[localX];

        if (column.baseType != BaseColumnType.Ground)
        {
            return false;
        }

        if (column.featureType == FeatureType.Platform)
        {
            return false;
        }

        for (int offset = -gapDistance; offset <= gapDistance; offset++)
        {
            int x = localX + offset;

            if (x < 0 || x >= columns.Length)
            {
                continue;
            }

            if (columns[x].baseType != BaseColumnType.Ground)
            {
                return false;
            }
        }

        return true;
    }

    private bool HasEnoughFlatGround(LevelColumnData[] columns, int localX, DecorationSize size)
    {
        int width = GetRequiredWidth(size);
        int groundHeight = columns[localX].groundHeight;
        int sideClearance = RequiresSideClearance(size) ? wideAndLargeSideClearance : 0;

        int firstRequiredX = localX - sideClearance;
        int lastRequiredX = localX + width - 1 + sideClearance;

        if (firstRequiredX < 0 || lastRequiredX >= columns.Length)
        {
            return false;
        }

        for (int x = firstRequiredX; x <= lastRequiredX; x++)
        {
            if (columns[x].baseType != BaseColumnType.Ground)
            {
                return false;
            }

            if (columns[x].groundHeight != groundHeight)
            {
                return false;
            }
        }

        return true;
    }

    private bool RequiresSideClearance(DecorationSize size)
    {
        return size == DecorationSize.Wide ||
               size == DecorationSize.Large;
    }

    private int GetRequiredWidth(DecorationSize size)
    {
        switch (size)
        {
            case DecorationSize.Wide:
                return 2;
            case DecorationSize.Large:
                return 3;
            default:
                return 1;
        }
    }

    private void SpawnPrefab(
        GameObject prefab,
        LevelColumnData column,
        int xOffset,
        int localX,
        Transform parent
    )
    {
        float worldX = xOffset + localX + 0.5f;
        float worldY = column.groundHeight + 1f;
        Vector3 spawnPosition = new Vector3(worldX, worldY, 0f);

        GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity, parent);

        SpriteRenderer spriteRenderer = instance.GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = sortingLayerName;
            spriteRenderer.sortingOrder = orderInLayer;

            if (randomFlipX && Random.value < 0.5f)
            {
                spriteRenderer.flipX = !spriteRenderer.flipX;
            }
        }
    }

    private GameObject GetRandomPrefab(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        return prefabs[Random.Range(0, prefabs.Length)];
    }

    private Transform GetOrCreateChunkParent(int chunkIndex)
    {
        if (chunkParents.TryGetValue(chunkIndex, out Transform existingParent) && existingParent != null)
        {
            return existingParent;
        }

        GameObject parentObject = new GameObject("Chunk_" + chunkIndex + "_Decorations");
        parentObject.transform.SetParent(transform);

        Transform parent = parentObject.transform;
        chunkParents[chunkIndex] = parent;

        return parent;
    }
}
