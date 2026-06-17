using UnityEngine;

public class HeightLayoutGenerator : MonoBehaviour
{
    [Header("Height settings")]
    [SerializeField] private int startGroundHeight = 1;
    [SerializeField] private int minGroundHeight = 1;
    [SerializeField] private int maxGroundHeight = 5;

    [Header("Trend settings")]
    [SerializeField] private int chanceToContinueTrend = 45;
    [SerializeField] private int chanceToStayFlat = 35;
    [SerializeField] private int chanceToReverseTrend = 15;
    [SerializeField] private int chanceForBigStep = 5;

    [Header("Big step settings")]
    [SerializeField] private int chanceForStepTwo = 80;
    [SerializeField] private int chanceForStepThree = 20;

    private int lastGroundHeight;
    private int lastTrend;
    private bool hasHeightState;

    public void ResetHeightState()
    {
        lastGroundHeight = startGroundHeight;
        lastTrend = 0;
        hasHeightState = false;
    }

    public void ApplyHeights(LevelColumnData[] columns)
    {
        if (columns == null)
        {
            Debug.LogError("HeightLayoutGenerator dostał null columns.");
            return;
        }

        int currentHeight = hasHeightState ? lastGroundHeight : startGroundHeight;
        int currentTrend = hasHeightState ? lastTrend : 0;

        bool previousWasLiquidGap = false;

        for (int x = 0; x < columns.Length; x++)
        {
            if (columns[x].baseType == BaseColumnType.Ground)
            {
                if (previousWasLiquidGap)
                {
                    // Po wodzie/lawie teren wraca na tę samą wysokość,
                    // żeby brzegi cieczy były płaskie.
                    columns[x].groundHeight = currentHeight;
                    currentTrend = 0;
                }
                else
                {
                    int heightChange = ChooseHeightChange(currentTrend);

                    int newHeight = currentHeight + heightChange;
                    newHeight = Mathf.Clamp(newHeight, minGroundHeight, maxGroundHeight);

                    int realChange = newHeight - currentHeight;

                    currentHeight = newHeight;
                    columns[x].groundHeight = currentHeight;

                    currentTrend = GetTrendFromChange(realChange);
                }

                previousWasLiquidGap = false;
            }
            else
            {
                columns[x].groundHeight = currentHeight;

                if (columns[x].baseType == BaseColumnType.WaterGap ||
                    columns[x].baseType == BaseColumnType.LavaGap)
                {
                    previousWasLiquidGap = true;
                }
            }
        }

        lastGroundHeight = currentHeight;
        lastTrend = currentTrend;
        hasHeightState = true;

        Debug.Log("HeightLayoutGenerator: ustawiono wysokości z pamięcią poprzedniego chunka.");
    }

    private int ChooseHeightChange(int currentTrend)
    {
        int roll = Random.Range(0, 100);

        if (roll < chanceToContinueTrend)
        {
            return ContinueTrend(currentTrend);
        }

        roll -= chanceToContinueTrend;

        if (roll < chanceToStayFlat)
        {
            return 0;
        }

        roll -= chanceToStayFlat;

        if (roll < chanceToReverseTrend)
        {
            return ReverseTrend(currentTrend);
        }

        roll -= chanceToReverseTrend;

        if (roll < chanceForBigStep)
        {
            return GetBigStep(currentTrend);
        }

        return 0;
    }

    private int ContinueTrend(int currentTrend)
    {
        if (currentTrend == 0)
        {
            int roll = Random.Range(0, 3);

            if (roll == 0) return -1;
            if (roll == 1) return 0;
            return 1;
        }

        return currentTrend;
    }

    private int ReverseTrend(int currentTrend)
    {
        if (currentTrend == 0)
        {
            int roll = Random.Range(0, 2);
            return roll == 0 ? -1 : 1;
        }

        return -currentTrend;
    }

    private int GetBigStep(int currentTrend)
    {
        int direction = currentTrend;

        if (direction == 0)
        {
            int rollDirection = Random.Range(0, 2);
            direction = rollDirection == 0 ? -1 : 1;
        }

        int roll = Random.Range(0, 100);
        int stepSize = roll < chanceForStepTwo ? 2 : 3;

        return direction * stepSize;
    }

    private int GetTrendFromChange(int change)
    {
        if (change > 0) return 1;
        if (change < 0) return -1;
        return 0;
    }

    public void ApplySettings(ChunkGenerationSettings settings)
    {
        if (settings == null)
        {
            return;
        }

        minGroundHeight = settings.minGroundHeight;
        maxGroundHeight = settings.maxGroundHeight;

        chanceToContinueTrend = settings.chanceToContinueTrend;
        chanceToStayFlat = settings.chanceToStayFlat;
        chanceToReverseTrend = settings.chanceToReverseTrend;
        chanceForBigStep = settings.chanceForBigStep;
    }

    public void ApplyHeights(LevelColumnData[] columns, ChunkGenerationSettings settings)
    {
        ApplySettings(settings);
        ApplyHeights(columns);
    }
}