using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private class SpawnZone
    {
        public int startX;
        public int endX;
        public int groundHeight;
        public bool hasPlatformAbove;

        public int Length
        {
            get { return endX - startX + 1; }
        }

        public int CenterX
        {
            get { return (startX + endX) / 2; }
        }
    }

    [Header("Enemy prefabs")]
    [SerializeField] private GameObject walkingEnemyPrefab;
    [SerializeField] private GameObject flyingEnemyPrefab;

    [Header("Spawn rules")]
    [SerializeField] private int minChunkIndexForEnemies = 1;
    [SerializeField] private int minFlatGroundLength = 4;
    [SerializeField] private int minDistanceFromChunkEdge = 5;
    [SerializeField] private int baseSpawnChancePercent = 40;
    [SerializeField] private int spawnChancePerDifficulty = 8;

    [Header("Enemy count")]
    [SerializeField] private int baseMaxEnemiesPerChunk = 2;
    [SerializeField] private int extraEnemiesPerDifficulty = 1;
    [SerializeField] private int absoluteMaxEnemiesPerChunk = 5;

    [Header("Enemy type weights")]
    [SerializeField] private int walkingWeightWithoutPlatform = 75;
    [SerializeField] private int flyingWeightWithoutPlatform = 25;
    [SerializeField] private int walkingWeightWithPlatform = 30;
    [SerializeField] private int flyingWeightWithPlatform = 70;

    [Header("Spawn offsets")]
    [SerializeField] private float walkingYOffset = 1f;
    [SerializeField] private float flyingYOffset = 3f;

    private readonly Dictionary<int, Transform> chunkParents = new Dictionary<int, Transform>();

    public void SpawnEnemies(LevelColumnData[] columns, int xOffset, int chunkIndex, int difficultyLevel)
    {
        if (columns == null || chunkIndex < minChunkIndexForEnemies)
        {
            return;
        }

        if (walkingEnemyPrefab == null && flyingEnemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: nie podpięto żadnego prefabu przeciwnika.");
            return;
        }

        List<SpawnZone> zones = FindSpawnZones(columns);

        if (zones.Count == 0)
        {
            return;
        }

        int spawnChance = Mathf.Clamp(
            baseSpawnChancePercent + difficultyLevel * spawnChancePerDifficulty,
            0,
            100
        );

        if (Random.Range(0, 100) >= spawnChance)
        {
            return;
        }

        int maxEnemiesThisChunk = Mathf.Clamp(
            baseMaxEnemiesPerChunk + difficultyLevel * extraEnemiesPerDifficulty,
            1,
            absoluteMaxEnemiesPerChunk
        );

        int enemiesToSpawn = Random.Range(1, maxEnemiesThisChunk + 1);
        enemiesToSpawn = Mathf.Min(enemiesToSpawn, zones.Count);

        Transform chunkParent = GetOrCreateChunkParent(chunkIndex);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            int zoneIndex = Random.Range(0, zones.Count);
            SpawnZone zone = zones[zoneIndex];
            zones.RemoveAt(zoneIndex);

            SpawnEnemyInZone(zone, xOffset, chunkParent);
        }
    }

    public void ClearEnemiesForChunk(int chunkIndex)
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

    public void ClearAllSpawnedEnemies()
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

    private List<SpawnZone> FindSpawnZones(LevelColumnData[] columns)
    {
        List<SpawnZone> zones = new List<SpawnZone>();

        int x = minDistanceFromChunkEdge;
        int lastAllowedX = columns.Length - 1 - minDistanceFromChunkEdge;

        while (x <= lastAllowedX)
        {
            if (!IsValidGroundColumn(columns, x))
            {
                x++;
                continue;
            }

            int startX = x;
            int groundHeight = columns[x].groundHeight;

            while (x <= lastAllowedX &&
                   IsValidGroundColumn(columns, x) &&
                   columns[x].groundHeight == groundHeight)
            {
                x++;
            }

            int endX = x - 1;
            int length = endX - startX + 1;

            if (length >= minFlatGroundLength)
            {
                zones.Add(new SpawnZone
                {
                    startX = startX,
                    endX = endX,
                    groundHeight = groundHeight,
                    hasPlatformAbove = HasPlatformAbove(columns, startX, endX, groundHeight)
                });
            }
        }

        return zones;
    }

    private bool IsValidGroundColumn(LevelColumnData[] columns, int x)
    {
        return columns[x].baseType == BaseColumnType.Ground;
    }

    private bool HasPlatformAbove(LevelColumnData[] columns, int startX, int endX, int groundHeight)
    {
        for (int x = startX; x <= endX; x++)
        {
            if (columns[x].featureType == FeatureType.Platform &&
                columns[x].platformHeight > groundHeight)
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnEnemyInZone(SpawnZone zone, int xOffset, Transform chunkParent)
    {
        GameObject prefab = ChooseEnemyPrefab(zone.hasPlatformAbove);

        if (prefab == null)
        {
            return;
        }

        int localX = Random.Range(zone.startX, zone.endX + 1);
        float worldX = xOffset + localX + 0.5f;

        bool isFlyingEnemy = prefab == flyingEnemyPrefab;
        float yOffset = isFlyingEnemy ? flyingYOffset : walkingYOffset;
        float worldY = zone.groundHeight + yOffset;

        Vector3 spawnPosition = new Vector3(worldX, worldY, 0f);
        Instantiate(prefab, spawnPosition, Quaternion.identity, chunkParent);
    }

    private GameObject ChooseEnemyPrefab(bool hasPlatformAbove)
    {
        if (walkingEnemyPrefab == null)
        {
            return flyingEnemyPrefab;
        }

        if (flyingEnemyPrefab == null)
        {
            return walkingEnemyPrefab;
        }

        int walkingWeight = hasPlatformAbove ? walkingWeightWithPlatform : walkingWeightWithoutPlatform;
        int flyingWeight = hasPlatformAbove ? flyingWeightWithPlatform : flyingWeightWithoutPlatform;

        int totalWeight = walkingWeight + flyingWeight;

        if (totalWeight <= 0)
        {
            return walkingEnemyPrefab;
        }

        int roll = Random.Range(0, totalWeight);

        if (roll < walkingWeight)
        {
            return walkingEnemyPrefab;
        }

        return flyingEnemyPrefab;
    }

    private Transform GetOrCreateChunkParent(int chunkIndex)
    {
        if (chunkParents.TryGetValue(chunkIndex, out Transform existingParent) && existingParent != null)
        {
            return existingParent;
        }

        GameObject parentObject = new GameObject("Chunk_" + chunkIndex + "_Enemies");
        parentObject.transform.SetParent(transform);

        Transform parent = parentObject.transform;
        chunkParents[chunkIndex] = parent;

        return parent;
    }
}
