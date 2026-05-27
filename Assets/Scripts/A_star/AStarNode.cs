using UnityEngine;

public class AStarNode : IHeapItem<AStarNode>
{
    public enum ENodeType { Obstacle, WalkableFloor, Air }

    public ENodeType NodeType;

    public bool IsWalkable => NodeType != ENodeType.Obstacle;

    public Vector3 WorldPosition;
    public int GridX;
    public int GridY;

    public int GCost;
    public int HCost;
    public int FCost => GCost + HCost;
    public AStarNode Parent;

    private int _heapIndex;
    public int HeapIndex
    {
        get => _heapIndex;
        set => _heapIndex = value;
    }

    public int CompareTo(AStarNode other)
    {
        int compare = FCost.CompareTo(other.FCost);
        if (compare == 0)
            compare = HCost.CompareTo(other.HCost);

        return -compare;
    }

    public AStarNode(ENodeType nodeType, Vector3 worldPosition, int gridX, int gridY)
    {
        NodeType = nodeType;
        WorldPosition = worldPosition;
        GridX = gridX;
        GridY = gridY;
    }

    public void ResetCosts()
    {
        GCost = 0;
        HCost = 0;
        Parent = null;
    }
}