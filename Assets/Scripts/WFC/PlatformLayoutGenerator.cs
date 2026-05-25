using UnityEngine;

public class PlatformLayoutGenerator : MonoBehaviour
{
    [Header("Forced platforms over gaps")]
    [SerializeField] private int minGapLengthForPlatform = 4;

    [Header("Random platforms")]
    [SerializeField] private int randomPlatformChance = 12;
    [SerializeField] private int maxRandomPlatforms = 6;

    [Header("Platform size")]
    [SerializeField] private int minPlatformLength = 2;
    [SerializeField] private int maxPlatformLength = 4;

    [Header("Platform height")]
    [SerializeField] private int minHeightAboveBase = 3;
    [SerializeField] private int maxHeightAboveBase = 5;

    [Header("Spacing")]
    [SerializeField] private int minDistanceBetweenPlatforms = 2;

    public void ApplyPlatforms(LevelColumnData[] columns)
    {
        if (columns == null)
        {
            Debug.LogError("PlatformLayoutGenerator dostał null columns.");
            return;
        }

        ClearPlatforms(columns);

        AddForcedPlatformsOverLongGaps(columns);
        AddRandomPlatforms(columns);

        Debug.Log("PlatformLayoutGenerator: dodano platformy.");
    }

    private void ClearPlatforms(LevelColumnData[] columns)
    {
        for (int x = 0; x < columns.Length; x++)
        {
            columns[x].featureType = FeatureType.None;
            columns[x].platformHeight = 0;
        }
    }

    private void AddForcedPlatformsOverLongGaps(LevelColumnData[] columns)
    {
        int x = 0;

        while (x < columns.Length)
        {
            if (!IsGapType(columns[x].baseType))
            {
                x++;
                continue;
            }

            int gapStart = x;

            while (x < columns.Length && IsGapType(columns[x].baseType))
            {
                x++;
            }

            int gapEnd = x - 1;
            int gapLength = gapEnd - gapStart + 1;

            if (gapLength >= minGapLengthForPlatform)
            {
                AddPlatformInGap(columns, gapStart, gapEnd);
            }
        }
    }

    private void AddPlatformInGap(LevelColumnData[] columns, int gapStart, int gapEnd)
    {
        int gapLength = gapEnd - gapStart + 1;

        int platformLength = Mathf.Clamp(
            Random.Range(minPlatformLength, maxPlatformLength + 1),
            1,
            gapLength
        );

        int gapCenter = (gapStart + gapEnd) / 2;
        int platformStart = gapCenter - platformLength / 2;

        platformStart = Mathf.Clamp(
            platformStart,
            gapStart,
            gapEnd - platformLength + 1
        );

        int baseHeight = columns[gapStart].groundHeight;
        int platformHeight = baseHeight + Random.Range(minHeightAboveBase, maxHeightAboveBase + 1);

        PlacePlatform(columns, platformStart, platformLength, platformHeight);
    }

    private void AddRandomPlatforms(LevelColumnData[] columns)
    {
        int createdPlatforms = 0;

        for (int x = 0; x < columns.Length; x++)
        {
            if (createdPlatforms >= maxRandomPlatforms)
            {
                return;
            }

            int roll = Random.Range(0, 100);

            if (roll >= randomPlatformChance)
            {
                continue;
            }

            int platformLength = Random.Range(minPlatformLength, maxPlatformLength + 1);

            if (!CanPlacePlatform(columns, x, platformLength))
            {
                continue;
            }

            int baseHeight = columns[x].groundHeight;
            int platformHeight = baseHeight + Random.Range(minHeightAboveBase, maxHeightAboveBase + 1);

            PlacePlatform(columns, x, platformLength, platformHeight);

            createdPlatforms++;
        }
    }

    private bool CanPlacePlatform(LevelColumnData[] columns, int startX, int length)
    {
        if (startX < 0 || startX + length > columns.Length)
        {
            return false;
        }

        int checkStart = Mathf.Max(0, startX - minDistanceBetweenPlatforms);
        int checkEnd = Mathf.Min(columns.Length - 1, startX + length - 1 + minDistanceBetweenPlatforms);

        for (int x = checkStart; x <= checkEnd; x++)
        {
            if (columns[x].featureType == FeatureType.Platform)
            {
                return false;
            }
        }

        return true;
    }

    private void PlacePlatform(LevelColumnData[] columns, int startX, int length, int platformHeight)
    {
        for (int x = startX; x < startX + length; x++)
        {
            if (x < 0 || x >= columns.Length)
            {
                continue;
            }

            columns[x].featureType = FeatureType.Platform;
            columns[x].platformHeight = platformHeight;
        }
    }

    private bool IsGapType(BaseColumnType type)
    {
        return type == BaseColumnType.Gap ||
               type == BaseColumnType.WaterGap ||
               type == BaseColumnType.LavaGap;
    }

    public void ApplySettings(ChunkGenerationSettings settings)
    {
        if (settings == null)
        {
            return;
        }

        minGapLengthForPlatform = settings.minGapLengthForPlatform;

        randomPlatformChance = settings.randomPlatformChance;
        maxRandomPlatforms = settings.maxRandomPlatforms;

        minPlatformLength = settings.minPlatformLength;
        maxPlatformLength = settings.maxPlatformLength;

        minHeightAboveBase = settings.minHeightAboveBase;
        maxHeightAboveBase = settings.maxHeightAboveBase;

        minDistanceBetweenPlatforms = settings.minDistanceBetweenPlatforms;
    }

    public void ApplyPlatforms(LevelColumnData[] columns, ChunkGenerationSettings settings)
    {
        ApplySettings(settings);
        ApplyPlatforms(columns);
    }
}