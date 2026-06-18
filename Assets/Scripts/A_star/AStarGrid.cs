using System.Collections.Generic;
using UnityEngine;

public class AStarGrid : MonoBehaviour
{
    [Header("Siatka")]
    public Vector2 GridWorldSize = new Vector2(20f, 20f);

    [Tooltip("Promien wezla.")]
    public float NodeRadius = 0.25f;

    [Header("Przeszkody")]
    [Tooltip("Warstwa solid tiles.")]
    public LayerMask ObstacleMask;

    [Tooltip("Jak daleko pod węzłem szukamy podlogi")]
    public float FloorCheckDistance = 0.6f;

    [Tooltip("Promien kola przy sprawdzaniu kolizji ze sciana ( < NodeRadius).")]
    public float ColliderCheckRadius = 0.1f;

    [Header("Debug")]
    public bool DrawGizmos = true;

    private AStarNode[,] _grid;
    private float _nodeDiameter;
    private int _gridSizeX;
    private int _gridSizeY;

    public int MaxSize => _gridSizeX * _gridSizeY;

    public int GridSizeX => _gridSizeX;
    public int GridSizeY => _gridSizeY;

    private void Awake()
    {
        RecalculateGridDimensions();
    }

    private void RecalculateGridDimensions()
    {
        _nodeDiameter = NodeRadius * 2f;
        _gridSizeX = Mathf.Max(1, Mathf.RoundToInt(GridWorldSize.x / _nodeDiameter));
        _gridSizeY = Mathf.Max(1, Mathf.RoundToInt(GridWorldSize.y / _nodeDiameter));
    }

    public void CreateGrid()
    {
        RecalculateGridDimensions();

        _grid = new AStarNode[_gridSizeX, _gridSizeY];

        Vector2 worldBottomLeft = (Vector2)transform.position
                                  - Vector2.right * (GridWorldSize.x / 2f)
                                  - Vector2.up * (GridWorldSize.y / 2f);

        for (int x = 0; x < _gridSizeX; x++)
        {
            for (int y = 0; y < _gridSizeY; y++)
            {
                Vector2 worldPoint = worldBottomLeft
                                     + Vector2.right * (x * _nodeDiameter + NodeRadius)
                                     + Vector2.up * (y * _nodeDiameter + NodeRadius);

                AStarNode.ENodeType nodeType = ClassifyNode(worldPoint);

                Vector3 worldPoint3D = new Vector3(worldPoint.x, worldPoint.y, 0f);
                _grid[x, y] = new AStarNode(nodeType, worldPoint3D, x, y);
            }
        }
    }

    private AStarNode.ENodeType ClassifyNode(Vector2 worldPoint)
    {
        // Sprawdz czy srodek wezla wewnatrz przeszkody
        Vector2 boxSize = new Vector2(_nodeDiameter - 0.05f, _nodeDiameter - 0.05f);
        bool insideObstacle = Physics2D.OverlapBox(worldPoint, boxSize, 0f, ObstacleMask);

        if (insideObstacle)
            return AStarNode.ENodeType.Obstacle;

        // Sprawdz czy pod wezlem jest podloga
        RaycastHit2D floorHit = Physics2D.Raycast(
            worldPoint,
            Vector2.down,
            FloorCheckDistance,
            ObstacleMask
        );

        if (floorHit.collider != null)
            return AStarNode.ENodeType.WalkableFloor;

        return AStarNode.ENodeType.Air;
    }

    public AStarNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = Mathf.Clamp01(
            (worldPosition.x - transform.position.x + GridWorldSize.x / 2f) / GridWorldSize.x);
        float percentY = Mathf.Clamp01(
            (worldPosition.y - transform.position.y + GridWorldSize.y / 2f) / GridWorldSize.y);

        int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

        return _grid[x, y];
    }

    public List<AStarNode> GetNeighbours(AStarNode node, bool canFly)
    {
        var neighbours = new List<AStarNode>(8);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = node.GridX + dx;
                int ny = node.GridY + dy;

                // Sprawdzenie granic mapy dla sąsiada
                if (nx < 0 || nx >= _gridSizeX || ny < 0 || ny >= _gridSizeY) continue;

                AStarNode neighbour = _grid[nx, ny];

                // Przeszkoda na węźle docelowym zawsze całkowicie blokuje wejście
                if (neighbour.NodeType == AStarNode.ENodeType.Obstacle) continue;

                // --- INTELIGENTNA BLOKADA RUCHÓW PO SKOSIE (Twój schemat) ---
                if (dx != 0 && dy != 0)
                {
                    // Szukamy dwóch węzłów bezpośrednio stykających się z obecną pozycją (node)
                    // w kierunku, w którym chcemy wykonać skos (dokładnie czerwone pola z rysunku)
                    int checkX = node.GridX + dx;
                    int checkY = node.GridY + dy;

                    // Upewniamy się, że pozycje sprawdzane nie wychodzą poza tablicę siatki
                    if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
                    {
                        AStarNode sideH = _grid[checkX, node.GridY]; // Blok obok (lewo/prawo)
                        AStarNode sideV = _grid[node.GridX, checkY]; // Blok obok (góra/dół)

                        // Jeśli choć jedno z tych pól to Obstacle, ruch po skosie grozi kolizją.
                        // Odrzucamy ten skos – A* zostanie zmuszony pójść pod kątem prostym (90 stopni).
                        if (sideH.NodeType == AStarNode.ENodeType.Obstacle ||
                            sideV.NodeType == AStarNode.ENodeType.Obstacle)
                        {
                            continue;
                        }
                    }
                }

                // Nielatający wróg tylko na WalkableFloor
                if (!canFly && neighbour.NodeType == AStarNode.ENodeType.Air) continue;

                neighbours.Add(neighbour);
            }
        }

        return neighbours;
    }

    public void ResetNodes()
    {
        if (_grid == null) return;
        foreach (var node in _grid)
            node.ResetCosts();
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = new Color(0.3f, 0.3f, 1f, 0.4f);
    //    Gizmos.DrawWireCube(transform.position,
    //                        new Vector3(GridWorldSize.x, GridWorldSize.y, 0.1f));

    //    if (!DrawGizmos || _grid == null) return;

    //    float size = _nodeDiameter - 0.04f;

    //    foreach (AStarNode node in _grid)
    //    {
    //        switch (node.NodeType)
    //        {
    //            case AStarNode.ENodeType.Obstacle:
    //                Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Czerwony
    //                break;

    //            case AStarNode.ENodeType.WalkableFloor:
    //                Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Zielony
    //                break;
    //        }

    //        Gizmos.DrawCube(node.WorldPosition, new Vector3(size, size, 0.01f));
    //    }
    //}

    private void Start()
    {
        CreateGrid();
    }
}
