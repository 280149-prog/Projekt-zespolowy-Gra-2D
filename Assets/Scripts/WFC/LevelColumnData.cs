using UnityEngine;

public enum BaseColumnType
{
    Ground,
    Gap,
    WaterGap,
    LavaGap
}

public enum FeatureType
{
    None,
    Platform
}

[System.Serializable]
public class LevelColumnData
{
    public BaseColumnType baseType;
    public int groundHeight;

    public FeatureType featureType;
    public int platformHeight;

    public LevelColumnData()
    {
        baseType = BaseColumnType.Ground;
        groundHeight = 2;

        featureType = FeatureType.None;
        platformHeight = 0;
    }

    public LevelColumnData(BaseColumnType baseType, int groundHeight)
    {
        this.baseType = baseType;
        this.groundHeight = groundHeight;

        featureType = FeatureType.None;
        platformHeight = 0;
    }
}