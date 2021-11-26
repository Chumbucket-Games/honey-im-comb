using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SquareGrid : MonoBehaviour
{
    [SerializeField] private uint rows = 5;
    [Range(0f, 10f)]
    [SerializeField] private float rowPadding = 1f;

    [SerializeField] private uint columns = 5;
    [Range(0f, 10f)]
    [SerializeField] private float columnPadding = 1f;

    [Space]
    [Space]
    [Header("Debug Options")]
    [SerializeField] private bool displayGrid = false;
    [SerializeField] private Color gridColor = Color.red;
    [SerializeField] private float heightOffset = 0;

    private Cell[,] grid;
    private Cell selectedCell = null;
    [Header("Occupied Cells")]
    [SerializeField] Vector2Int[] walls;
    [SerializeField] GridObject[] overworldObjects;

    public bool IsGridFull { get; private set; }
    public uint TotalCells { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        grid = new Cell[rows, columns];
        InitGridCells();

        IsGridFull = false;
        TotalCells = rows * columns;
        SetWalls();

        WallAllObjectCells();
    }

    public void SetWalls()
    {
        foreach (var wall in walls)
        {
            grid[wall.y, wall.x].SetWall();
        }
    }

    public void WallAllObjectCells()
    {
        foreach (var go in overworldObjects)
        {
            Vector2Int centerCell = WorldToCell(go.transform.position);
            if (go.dimensions.x % 2 == 0 && go.dimensions.y % 2 == 0)
            {
                // Even-numbered grid arrangements will treat the middle of the middle 4 cells as the center (which floors to the bottom left cell of the center).
                // Otherwise, we treat the center point as the center cell.
                // The bottom left needs to be offset by 1 cell in this calculation.
                Vector2Int bottomLeft = new Vector2Int(centerCell.x - (go.dimensions.x / 2) + 1, centerCell.y - (go.dimensions.y / 2) + 1);
                Vector2Int topRight = new Vector2Int(centerCell.x + (go.dimensions.x / 2), centerCell.y + (go.dimensions.y / 2));
                for (int x = bottomLeft.x; x <= topRight.x; x++)
                {
                    for (int y = bottomLeft.y; y <= topRight.y; y++)
                    {
                        grid[y, x].SetWall();
                    }
                }
            }
            else
            {
                Vector2Int bottomLeft = new Vector2Int(centerCell.x - (go.dimensions.x / 2), centerCell.y - (go.dimensions.y / 2));
                Vector2Int topRight = new Vector2Int(centerCell.x + (go.dimensions.x / 2), centerCell.y + (go.dimensions.y / 2));
                for (int x = bottomLeft.x; x <= topRight.x; x++)
                {
                    for (int y = bottomLeft.y; y <= topRight.y; y++)
                    {
                        grid[y, x].SetWall();
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        if (!displayGrid) return;

        if (!Application.isPlaying)
        {
            grid = new Cell[rows, columns];
            InitGridCells();
            SetWalls();
            WallAllObjectCells();
        }

        DrawCellsGizmos();
    }

    private void InitGridCells()
    {
        for (int row = 0; row < rows; row++)
        {
            float posZ = transform.position.z - (rows * rowPadding / 2f) + rowPadding / 2f;
            float rowPos = posZ + (row * rowPadding);

            for (int col = 0; col < columns; col++)
            {
                float posX = transform.position.x - (columns * columnPadding / 2f) + columnPadding / 2f;
                float colPos = posX + (col * columnPadding);

                var position = new Vector3(colPos, transform.position.y, rowPos);
                grid[row, col] = new Cell(row, col, position, this);
            }
        }
    }

    public Vector2Int WorldToCell(Vector3 position)
    {
        var rowValue = (position.z - (gameObject.transform.position.z - rowPadding * rows / 2f)) / rowPadding;
        var colValue = (position.x - (gameObject.transform.position.x - columnPadding * columns / 2f)) / columnPadding;

        var row = Mathf.FloorToInt(rowValue);
        var col = Mathf.FloorToInt(colValue);

        row = Mathf.Clamp(row, 0, (int)rows - 1);
        col = Mathf.Clamp(col, 0, (int)columns - 1);
        return new Vector2Int(col, row);
    }

    public Cell GetCell(Vector2Int cellPosition)
    {
        return grid[cellPosition.y, cellPosition.x];
    }

    public Cell GetClosestAvailableCellToPosition(Vector3 position)
    {
        Debug.DrawLine(new Vector3(position.x - 5f, 5f, position.z), new Vector3(position.x + 5f, 5f, position.z), Color.magenta);
        Debug.DrawLine(new Vector3(position.x, 5f, position.z - 5f), new Vector3(position.x, 5f, position.z + 5f), Color.magenta);

        Vector2Int cell = WorldToCell(position);

        var startCell = grid[cell.y, cell.x];

        if (!startCell.IsOccupied && !startCell.IsWall)
        {
            selectedCell = grid[cell.y, cell.x];
            return grid[cell.y, cell.x];
        }

        // continue the search
        var unoccupiedCell = SearchSurroundingCellsForAvailable(startCell);
        selectedCell = unoccupiedCell;

        return unoccupiedCell;
    }

    // NOTE: Could be further optimized by storing the current row and col offset on the wave
    /// <summary>
    /// Searches for the nearest unoccupied cell by percolating outwards from the start cell.
    /// </summary>
    /// <param name="startCell">The start cell.</param>
    /// <returns>The first unoccupied cell found.</returns>
    private Cell SearchSurroundingCellsForAvailable(Cell startCell)
    {
        uint maxOffset = (rows > columns) ? rows : columns;
        Vector2Int startCoords = new Vector2Int(startCell.ColIndex, startCell.RowIndex);
        for (int offset = 1; offset <= maxOffset; offset++)
        {
            for (int y = offset; y >= -offset; y--)
            {
                for (int x = -offset; x <= offset; x++)
                {
                    var index = new Vector2Int(startCoords.x + x, startCoords.y + y);
                    if (index.x >= 0 && index.x < columns && index.y >= 0 && index.y < rows
                        && index != startCoords)
                    {
                        var cell = grid[index.y, index.x];
                        if (!cell.IsOccupied && !cell.IsWall)
                        {
                            Debug.Log($"Found unoccupied cell: ({cell.ColIndex}, {cell.RowIndex})");
                            return cell;
                        }
                    }
                }
            }
        }

        return null;
    }

    private void DrawCellsGizmos()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (selectedCell != null && selectedCell.RowIndex == row && selectedCell.ColIndex == col)
                {
                    grid[row, col].DrawCellGizmos(rowPadding - .5f, columnPadding - .5f, Color.magenta, heightOffset);
                }
                else
                {
                    grid[row, col].DrawCellGizmos(rowPadding, columnPadding, gridColor, heightOffset);
                }
            }
        }
    }

    public static Stack<Cell> FindPath(Node startNode, Node targetNode)
    {
        var toSearch = new List<Node> { startNode };
        var processed = new List<Node>();
        var direction = Vector3.zero;

        while (toSearch.Any())
        {
            // The first entry in the list has the current 'best' F value (or H value if they're equal).
            // This is achieved by sorting the list of nodes once the list has been populated.
            var current = toSearch[0]; 
            processed.Add(current);
            toSearch.Remove(current);

            if (current.cell == targetNode.cell)
            {
                var currentPathTile = current;
                var path = new Stack<Cell>(); // Using a stack ensures we have the list of moves in the right order.
                Debug.Log($"Start cell: ({startNode.cell.ColIndex}, {startNode.cell.RowIndex})");
                
                while (currentPathTile.cell != startNode.cell)
                {
                    var currentDirection = Vector3.zero;
                    // If the cell on the top of the stack is a straight line on a single axis
                    if (path.TryPeek(out var result))
                    {
                        currentDirection = (currentPathTile.cell.Position - result.Position).normalized;
                        if (currentDirection == direction)
                        {
                            path.Pop();
                        }
                        direction = currentDirection;
                    }
                    path.Push(currentPathTile.cell);
                    currentPathTile = currentPathTile.Connection;
                    
                    Debug.Log($"Current cell: ({currentPathTile.cell.ColIndex}, {currentPathTile.cell.RowIndex})");
                }

                return path;
            }

            foreach (var neighbor in current.Neighbors.Where(t => !t.cell.IsWall && !processed.Contains(t)))
            {
                var inSearch = toSearch.Contains(neighbor);
                var costToNeighbor = current.G + current.GetDistance(neighbor);

                if (!inSearch || costToNeighbor < neighbor.G)
                {
                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current);

                    if (!inSearch)
                    {
                        neighbor.SetH(neighbor.GetDistance(targetNode));
                        toSearch.Add(neighbor);
                    }
                }
            }
            // After filling the list, sort it
            toSearch.Sort(CompareNodes);
        }
        return null;
    }

    public uint GetRows()
    {
        return rows;
    }

    public uint GetColumns()
    {
        return columns;
    }

    public Cell GetCell(int x, int y)
    {
        return grid[y, x];
    }

    static int CompareNodes(Node a, Node b)
    {
        if (a.F < b.F)
        {
            return -1;
        }
        else if (a.F == b.F)
        {
            if (a.H < b.H)
            {
                return -1;
            }
            else if (a.H > b.H)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return 1;
        }
    }
}
