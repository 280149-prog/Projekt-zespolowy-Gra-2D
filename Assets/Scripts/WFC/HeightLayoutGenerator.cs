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

    public void ApplyHeights(LevelColumnData[] columns)
    {
        if (columns == null)
        {
            Debug.LogError("HeightLayoutGenerator dostał null columns.");
            return;
        }

        int currentHeight = startGroundHeight;

        // trend:
        //  1 = teren szedł w górę
        //  0 = teren był płaski
        // -1 = teren szedł w dół
        int currentTrend = 0;

        for (int x = 0; x < columns.Length; x++)
        {
            if (columns[x].baseType == BaseColumnType.Ground)
            {
                int heightChange = ChooseHeightChange(currentTrend);

                int newHeight = currentHeight + heightChange;
                newHeight = Mathf.Clamp(newHeight, minGroundHeight, maxGroundHeight);

                // Jeśli clamp zablokował zmianę, to traktujemy to jako płasko.
                int realChange = newHeight - currentHeight;

                currentHeight = newHeight;
                columns[x].groundHeight = currentHeight;

                currentTrend = GetTrendFromChange(realChange);
            }
            else
            {
                // Gap / WaterGap / LavaGap nie mają normalnego gruntu,
                // ale zapamiętują wysokość ostatniego gruntu.
                // Dzięki temu woda/lawa może być rysowana na poziomie poprzedniej ziemi.
                columns[x].groundHeight = currentHeight;

                // Trend zostawiamy bez zmian.
                // Dzięki temu po przeszkodzie teren może kontynuować poprzedni kierunek.
            }
        }

        Debug.Log("HeightLayoutGenerator: ustawiono wysokości z trendem.");
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
            // Jeśli nie mamy trendu, wybierz delikatnie górę/dół albo płasko.
            int roll = Random.Range(0, 3);

            if (roll == 0)
            {
                return -1;
            }

            if (roll == 1)
            {
                return 0;
            }

            return 1;
        }

        return currentTrend;
    }

    private int ReverseTrend(int currentTrend)
    {
        if (currentTrend == 0)
        {
            int roll = Random.Range(0, 2);

            if (roll == 0)
            {
                return -1;
            }

            return 1;
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

        int stepSize;

        if (roll < chanceForStepTwo)
        {
            stepSize = 2;
        }
        else
        {
            stepSize = 3;
        }

        return direction * stepSize;
    }

    private int GetTrendFromChange(int change)
    {
        if (change > 0)
        {
            return 1;
        }

        if (change < 0)
        {
            return -1;
        }

        return 0;
    }
}