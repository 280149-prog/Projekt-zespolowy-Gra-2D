using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [Header("Chunk settings")]
    [SerializeField] private int chunkWidth = 64;
    [SerializeField] private int chunksAhead = 3;

    [Header("Player")]
    [SerializeField] private Transform playerTransform;

    [Header("Biome settings")]
    [SerializeField] private BiomeTileSet[] biomes;
    [SerializeField] private int biomeLengthInChunks = 2;

    [Header("Generator references")]
    [SerializeField] private BaseLayoutGenerator baseLayoutGenerator;
    [SerializeField] private HeightLayoutGenerator heightLayoutGenerator;
    [SerializeField] private PlatformLayoutGenerator platformLayoutGenerator;
    [SerializeField] private TilemapDrawer tilemapDrawer;

    private int lastGeneratedChunkIndex = -1;

    private void Start()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        tilemapDrawer.ClearTilemap();

        // Spawn rysujemy raz, osobno od chunków.
        tilemapDrawer.DrawSpawn(GetBiomeForChunk(0));

        GenerateInitialChunks();
    }

    private void Update()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        int playerChunkIndex = GetPlayerChunkIndex();

        // Jeśli chunksAhead = 3:
        // gracz w chunku 0 -> mamy mieć wygenerowane 0, 1, 2
        // gracz w chunku 1 -> mamy mieć wygenerowane 0, 1, 2, 3
        int desiredLastChunkIndex = playerChunkIndex + chunksAhead - 1;

        while (lastGeneratedChunkIndex < desiredLastChunkIndex)
        {
            GenerateNextChunk();
        }
    }

    private void GenerateInitialChunks()
    {
        int playerChunkIndex = GetPlayerChunkIndex();
        int desiredLastChunkIndex = playerChunkIndex + chunksAhead - 1;

        while (lastGeneratedChunkIndex < desiredLastChunkIndex)
        {
            GenerateNextChunk();
        }
    }

    private void GenerateNextChunk()
    {
        int chunkIndex = lastGeneratedChunkIndex + 1;
        GenerateChunk(chunkIndex);
        lastGeneratedChunkIndex = chunkIndex;
    }

    private void GenerateChunk(int chunkIndex)
    {
        int xOffset = chunkIndex * chunkWidth;

        BiomeTileSet biome = GetBiomeForChunk(chunkIndex);
        ChunkGenerationSettings settings = GetDifficultySettings(chunkIndex);

        LevelColumnData[] columns = baseLayoutGenerator.GenerateBaseLayoutData(chunkWidth, settings);

        heightLayoutGenerator.ApplyHeights(columns, settings);
        platformLayoutGenerator.ApplyPlatforms(columns, settings);

        tilemapDrawer.DrawColumns(columns, xOffset, biome);

        Debug.Log(
            "ChunkManager: wygenerowano chunk " + chunkIndex +
            ", xOffset=" + xOffset +
            ", biome=" + biome.biomeName +
            ", difficulty=" + GetDifficultyLevel(chunkIndex)
        );
    }

    private int GetPlayerChunkIndex()
    {
        if (playerTransform == null)
        {
            return 0;
        }

        int chunkIndex = Mathf.FloorToInt(playerTransform.position.x / chunkWidth);

        // Spawn jest na minusowych x, więc przed x=0 traktujemy gracza jak chunk 0.
        return Mathf.Max(0, chunkIndex);
    }

    private BiomeTileSet GetBiomeForChunk(int chunkIndex)
    {
        if (biomes == null || biomes.Length == 0)
        {
            Debug.LogError("ChunkManager: brak biomów w tablicy biomes.");
            return null;
        }

        if (biomeLengthInChunks <= 0)
        {
            biomeLengthInChunks = 1;
        }

        int biomeIndex = (chunkIndex / biomeLengthInChunks) % biomes.Length;
        return biomes[biomeIndex];
    }

    private int GetDifficultyLevel(int chunkIndex)
    {
        // Co 10 chunków podbijamy poziom trudności.
        return chunkIndex / 4;
    }

    private ChunkGenerationSettings GetDifficultySettings(int chunkIndex)
    {
        int difficulty = GetDifficultyLevel(chunkIndex);

        ChunkGenerationSettings settings = ChunkGenerationSettings.Default();

        // Base layout
        settings.minGapLength = Mathf.Clamp(2 + difficulty / 2, 2, 4);
        settings.maxGapLength = Mathf.Clamp(4 + difficulty, 4, 8);

        // Im trudniej, tym mniej bezpiecznego gruntu po przeszkodzie.
        settings.minGroundRunAfterGap = Mathf.Clamp(3 - difficulty / 2, 1, 3);

        // Height layout
        settings.minGroundHeight = 1;
        settings.maxGroundHeight = Mathf.Clamp(5 + difficulty, 5, 9);

        settings.chanceToContinueTrend = Mathf.Clamp(45 + difficulty * 2, 45, 60);
        settings.chanceToStayFlat = Mathf.Clamp(35 - difficulty * 2, 20, 35);
        settings.chanceToReverseTrend = Mathf.Clamp(15, 10, 20);
        settings.chanceForBigStep = Mathf.Clamp(5 + difficulty * 2, 5, 18);

        // Platforms
        // Przy trudniejszych chunkach platform może być trochę więcej,
        // bo większe gappy i wysokości wymagają pomocy w przejściu.
        settings.minGapLengthForPlatform = Mathf.Clamp(4, 3, 5);
        settings.randomPlatformChance = Mathf.Clamp(6 + difficulty * 2, 6, 20);
        settings.maxRandomPlatforms = Mathf.Clamp(1 + difficulty, 1, 10);

        settings.minPlatformLength = 2;
        settings.maxPlatformLength = Mathf.Clamp(3 + difficulty / 2, 2, 6);

        settings.minHeightAboveBase = 3;
        settings.maxHeightAboveBase = Mathf.Clamp(5 + difficulty / 2, 5, 7);

        settings.minDistanceBetweenPlatforms = Mathf.Clamp(4 - difficulty / 2, 2, 4);

        return settings;
    }

    private bool HasRequiredReferences()
    {
        if (baseLayoutGenerator == null)
        {
            Debug.LogError("ChunkManager: nie podpięto BaseLayoutGenerator.");
            return false;
        }

        if (heightLayoutGenerator == null)
        {
            Debug.LogError("ChunkManager: nie podpięto HeightLayoutGenerator.");
            return false;
        }

        if (platformLayoutGenerator == null)
        {
            Debug.LogError("ChunkManager: nie podpięto PlatformLayoutGenerator.");
            return false;
        }

        if (tilemapDrawer == null)
        {
            Debug.LogError("ChunkManager: nie podpięto TilemapDrawer.");
            return false;
        }

        if (biomes == null || biomes.Length == 0)
        {
            Debug.LogError("ChunkManager: nie ustawiono biomów.");
            return false;
        }

        return true;
    }
}