using UnityEngine;
using UnityEngine.InputSystem;

public class BaseLayoutGenerator : MonoBehaviour
{
    [Header("Chunk settings")]
    [SerializeField] private int width = 64;

    [Header("Base layout settings")]
    [SerializeField] private int startGroundLength = 3;
    [SerializeField] private int minGapLength = 2;
    [SerializeField] private int maxGapLength = 5;
    [SerializeField] private int minGroundRunAfterGap = 3;

    [Header("Pipeline references")]
    [SerializeField] private HeightLayoutGenerator heightLayoutGenerator;
    [SerializeField] private PlatformLayoutGenerator platformLayoutGenerator;
    [SerializeField] private TilemapDrawer tilemapDrawer;

    private LevelColumnData[] columns;

    private void Start()
    {
        Debug.Log("BaseLayoutGenerator działa. Kliknij R, żeby wygenerować chunk.");
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            GenerateChunk();
        }
    }

    private void GenerateChunk()
    {
        Debug.Log("Generuję Base Layout.");

        columns = GenerateBaseLayoutData();

        if (heightLayoutGenerator != null)
        {
            heightLayoutGenerator.ApplyHeights(columns);
        }
        else
        {
            Debug.LogWarning("Nie podpięto HeightLayoutGenerator.");
        }

        if (platformLayoutGenerator != null)
        {
            platformLayoutGenerator.ApplyPlatforms(columns);
        }
        else
        {
            Debug.LogWarning("Nie podpięto PlatformLayoutGenerator.");
        }

        if (tilemapDrawer != null)
        {
            tilemapDrawer.DrawColumns(columns);
        }
        else
        {
            Debug.LogError("Nie podpięto TilemapDrawer.");
        }

        PrintBaseLayout(columns);
    }

    public void ApplySettings(ChunkGenerationSettings settings)
    {
        if (settings == null)
        {
            return;
        }

        minGapLength = settings.minGapLength;
        maxGapLength = settings.maxGapLength;
        minGroundRunAfterGap = settings.minGroundRunAfterGap;
    }

    public LevelColumnData[] GenerateBaseLayoutData()
    {
        LevelColumnData[] result = new LevelColumnData[width];

        BaseColumnType previousType = BaseColumnType.Ground;

        int currentGapLength = 0;
        int forcedGroundCounter = 0;

        int forcedGapCounter = 0;
        BaseColumnType forcedGapType = BaseColumnType.Gap;

        for (int x = 0; x < width; x++)
        {
            LevelColumnData column = new LevelColumnData();

            if (x < startGroundLength)
            {
                column.baseType = BaseColumnType.Ground;

                currentGapLength = 0;
                forcedGroundCounter = 0;
                forcedGapCounter = 0;
            }
            else if (forcedGroundCounter > 0)
            {
                column.baseType = BaseColumnType.Ground;

                forcedGroundCounter--;
                currentGapLength = 0;
                forcedGapCounter = 0;
            }
            else if (forcedGapCounter > 0)
            {
                column.baseType = forcedGapType;

                forcedGapCounter--;
                currentGapLength++;
            }
            else
            {
                BaseColumnType selectedType = ChooseBaseType(previousType, currentGapLength);

                column.baseType = selectedType;

                if (IsGapType(selectedType))
                {
                    currentGapLength++;

                    // Jeśli dopiero weszliśmy w gap po groundzie,
                    // wymuszamy minimalną długość tego samego typu.
                    if (!IsGapType(previousType))
                    {
                        forcedGapType = selectedType;
                        forcedGapCounter = minGapLength - 1;
                    }
                }
                else
                {
                    if (IsGapType(previousType))
                    {
                        forcedGroundCounter = minGroundRunAfterGap - 1;
                    }

                    currentGapLength = 0;
                    forcedGapCounter = 0;
                }

                previousType = selectedType;
            }

            previousType = column.baseType;
            result[x] = column;
        }

        return result;
    }

    public LevelColumnData[] GenerateBaseLayoutData(int customWidth, ChunkGenerationSettings settings)
    {
        int previousWidth = width;

        ApplySettings(settings);

        width = customWidth;
        LevelColumnData[] result = GenerateBaseLayoutData();

        width = previousWidth;

        return result;
    }

    private BaseColumnType ChooseBaseType(BaseColumnType previousType, int currentGapLength)
    {
        // Jeśli gap jest już za długi, wymuszamy Ground,
        // żeby nie robić nieprzeskakiwalnej przepaści.
        if (currentGapLength >= maxGapLength)
        {
            return BaseColumnType.Ground;
        }

        int groundWeight = 75;
        int gapWeight = 10;
        int waterWeight = 7;
        int lavaWeight = 7;

        // Ważone sąsiedztwo:
        // po wodzie chętniej robimy wodę,
        // po lawie chętniej robimy lawę.
        if (previousType == BaseColumnType.WaterGap)
        {
            groundWeight = 25;
            gapWeight = 0;
            waterWeight = 70;
            lavaWeight = 0;
        }
        else if (previousType == BaseColumnType.LavaGap)
        {
            groundWeight = 25;
            gapWeight = 0;
            waterWeight = 0;
            lavaWeight = 70;
        }
        else if (previousType == BaseColumnType.Gap)
        {
            groundWeight = 45;
            gapWeight = 45;
            waterWeight = 0;
            lavaWeight = 0;
        }
        else if (previousType == BaseColumnType.Ground)
        {
            groundWeight = 80;
            gapWeight = 8;
            waterWeight = 6;
            lavaWeight = 6;
        }

        int totalWeight = groundWeight + gapWeight + waterWeight + lavaWeight;
        int roll = Random.Range(0, totalWeight);

        if (roll < groundWeight)
        {
            return BaseColumnType.Ground;
        }

        roll -= groundWeight;

        if (roll < gapWeight)
        {
            return BaseColumnType.Gap;
        }

        roll -= gapWeight;

        if (roll < waterWeight)
        {
            return BaseColumnType.WaterGap;
        }

        return BaseColumnType.LavaGap;
    }

    private bool IsGapType(BaseColumnType type)
    {
        return type == BaseColumnType.Gap ||
               type == BaseColumnType.WaterGap ||
               type == BaseColumnType.LavaGap;
    }

    private void PrintBaseLayout(LevelColumnData[] data)
    {
        string text = "Base layout: ";

        for (int i = 0; i < data.Length; i++)
        {
            text += ShortName(data[i].baseType);

            if (i < data.Length - 1)
            {
                text += " ";
            }
        }

        Debug.Log(text);
    }

    private string ShortName(BaseColumnType type)
    {
        if (type == BaseColumnType.Ground)
        {
            return "G";
        }

        if (type == BaseColumnType.Gap)
        {
            return "_";
        }

        if (type == BaseColumnType.WaterGap)
        {
            return "W";
        }

        if (type == BaseColumnType.LavaGap)
        {
            return "L";
        }

        return "?";
    }
}