using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AStarGrid))]
public class AStarPathfinder : MonoBehaviour
{
    private AStarGrid _grid;

    private void Awake()
    {
        _grid = GetComponent<AStarGrid>();
    }

    public List<Vector3> FindPath(Vector3 startWorldPos, Vector3 targetWorldPos, bool canFly = false)
    {
        // Reset kosztow z poprzedniego wywolania
        _grid.ResetNodes();

        AStarNode startNode = _grid.NodeFromWorldPoint(startWorldPos);
        AStarNode targetNode = _grid.NodeFromWorldPoint(targetWorldPos);

        // Jesli start lub cel jest w przeszkodzie — brak sciezki
        if (!startNode.IsWalkable || !targetNode.IsWalkable)
        {
            targetNode = FindNearestWalkable(targetNode, canFly);

            if (targetNode == null)
            {
                Debug.LogWarning("[A*] Start lub cel w przeszkodzie, brak alternatywy.");
                return new List<Vector3>();
            }
        }

        var openSet = new MinHeap<AStarNode>(_grid.MaxSize);
        var closedSet = new HashSet<AStarNode>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            AStarNode current = openSet.RemoveFirst();
            closedSet.Add(current);

            if (current == targetNode)
                return SmoothedPath(RetracePath(startNode, targetNode));

            foreach (AStarNode neighbour in _grid.GetNeighbours(current, canFly))
            {
                if (closedSet.Contains(neighbour)) continue;

                int moveCost = current.GCost + GetDistance(current, neighbour);

                // Dla nielatajacych wrogow skok kosztuje wiecej - TODO
                // A* preferuje chodzenie poziome nad skakaniem
                if (!canFly && neighbour.GridY > current.GridY)
                    moveCost += 5;

                if (moveCost < neighbour.GCost || !openSet.Contains(neighbour))
                {
                    neighbour.GCost = moveCost;
                    neighbour.HCost = GetDistance(neighbour, targetNode);
                    neighbour.Parent = current;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour); // obnizyl FCost - przebuduj kopiec
                }
            }
        }

        //Debug.LogWarning("[A*] Nie znaleziono sciezki.");
        return new List<Vector3>();
    }

    public static List<Vector3> SmoothedPath(List<Vector3> rawPath)
    {
        if (rawPath.Count <= 2) return rawPath;

        var result = new List<Vector3> { rawPath[0] };

        for (int i = 1; i < rawPath.Count - 1; i++)
        {
            Vector2 dirIn = ((Vector2)rawPath[i] - (Vector2)rawPath[i - 1]).normalized;
            Vector2 dirOut = ((Vector2)rawPath[i + 1] - (Vector2)rawPath[i]).normalized;

            // Jesli kierunek bez zmian — pomin punkt (wygladza proste odcinki)
            float dot = Vector2.Dot(dirIn, dirOut);
            if (dot < 0.99f) // zakret
                result.Add(rawPath[i]);
        }

        result.Add(rawPath[rawPath.Count - 1]);
        return result;
    }

    public static List<(Vector3 position, bool isCorner)> GetSmoothedPathWithFlags(List<Vector3> rawPath)
    {
        var result = new List<(Vector3, bool)>();
        if (rawPath.Count == 0) return result;

        result.Add((rawPath[0], false));

        for (int i = 1; i < rawPath.Count - 1; i++)
        {
            Vector2 dirIn = ((Vector2)rawPath[i] - (Vector2)rawPath[i - 1]).normalized;
            Vector2 dirOut = ((Vector2)rawPath[i + 1] - (Vector2)rawPath[i]).normalized;

            float dot = Vector2.Dot(dirIn, dirOut);
            bool isCorner = dot < 0.99f;

            if (isCorner)
                result.Add((rawPath[i], true));
        }

        result.Add((rawPath[rawPath.Count - 1], false));
        return result;
    }

    private List<Vector3> RetracePath(AStarNode startNode, AStarNode endNode)
    {
        var path = new List<Vector3>();
        var current = endNode;

        while (current != startNode)
        {
            path.Add(current.WorldPosition);
            current = current.Parent;
        }

        path.Add(startNode.WorldPosition);
        path.Reverse();
        return path;
    }

    private int GetDistance(AStarNode a, AStarNode b)
    {
        int distX = Mathf.Abs(a.GridX - b.GridX);
        int distY = Mathf.Abs(a.GridY - b.GridY);
        int diagonal = Mathf.Min(distX, distY);
        int straight = Mathf.Abs(distX - distY);
        return 14 * diagonal + 10 * straight;
    }

    private AStarNode FindNearestWalkable(AStarNode origin, bool canFly)
    {
        for (int radius = 1; radius <= 5; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius) continue;

                    int nx = origin.GridX + dx;
                    int ny = origin.GridY + dy;

                    if (nx < 0 || nx >= _grid.GridSizeX) continue;
                    if (ny < 0 || ny >= _grid.GridSizeY) continue;

                    var candidate = _grid.NodeFromWorldPoint(
                        new Vector3(origin.WorldPosition.x + dx * 0.5f,
                                    origin.WorldPosition.y + dy * 0.5f, 0f));

                    if (candidate == null) continue;
                    if (!candidate.IsWalkable) continue;
                    if (!canFly && candidate.NodeType != AStarNode.ENodeType.WalkableFloor) continue;

                    return candidate;
                }
            }
        }
        return null;
    }
}