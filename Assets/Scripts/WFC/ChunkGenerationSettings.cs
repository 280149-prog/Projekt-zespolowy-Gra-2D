[System.Serializable]
public class ChunkGenerationSettings
{
    // Base layout
    public int minGapLength;
    public int maxGapLength;
    public int minGroundRunAfterGap;

    // Height layout
    public int minGroundHeight;
    public int maxGroundHeight;
    public int chanceToContinueTrend;
    public int chanceToStayFlat;
    public int chanceToReverseTrend;
    public int chanceForBigStep;

    // Platforms
    public int minGapLengthForPlatform;
    public int randomPlatformChance;
    public int maxRandomPlatforms;
    public int minPlatformLength;
    public int maxPlatformLength;
    public int minHeightAboveBase;
    public int maxHeightAboveBase;
    public int minDistanceBetweenPlatforms;

    public static ChunkGenerationSettings Default()
    {
        ChunkGenerationSettings settings = new ChunkGenerationSettings();

        settings.minGapLength = 2;
        settings.maxGapLength = 5;
        settings.minGroundRunAfterGap = 3;

        settings.minGroundHeight = 1;
        settings.maxGroundHeight = 5;
        settings.chanceToContinueTrend = 45;
        settings.chanceToStayFlat = 35;
        settings.chanceToReverseTrend = 15;
        settings.chanceForBigStep = 5;

        settings.minGapLengthForPlatform = 4;
        settings.randomPlatformChance = 12;
        settings.maxRandomPlatforms = 6;
        settings.minPlatformLength = 2;
        settings.maxPlatformLength = 4;
        settings.minHeightAboveBase = 3;
        settings.maxHeightAboveBase = 5;
        settings.minDistanceBetweenPlatforms = 2;

        return settings;
    }
}