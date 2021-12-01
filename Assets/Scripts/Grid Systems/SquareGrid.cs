using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SquareGrid : MonoBehaviour
{
    [SerializeField] GridLayout layout;
    [Range(0f, 10f)]
    [SerializeField] private float rowPadding = 1f;
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
    [SerializeField] GridObject[] overworldObjects;

    public bool IsGridFull { get; private set; }
    public int TotalCells { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        grid = new Cell[layout.rows, layout.columns];
        InitGridCells();

        IsGridFull = false;
        TotalCells = layout.rows * layout.columns;

        WallAllObjectCells();
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
            grid = new Cell[layout.rows, layout.columns];
            InitGridCells();
            WallAllObjectCells();
        }

        DrawCellsGizmos();
    }

    private void InitGridCells()
    {
        foreach (var cellPreset in layout.Cells)
        {
            float posZ = transform.position.z - (layout.rows * rowPadding / 2f) + rowPadding / 2f;
            float rowPos = posZ + (cellPreset.coords.y * rowPadding);

            float posX = transform.position.x - (layout.columns * columnPadding / 2f) + columnPadding / 2f;
            float colPos = posX + (cellPreset.coords.x * columnPadding);

            var position = new Vector3(colPos, transform.position.y, rowPos);
            grid[cellPreset.coords.y, cellPreset.coords.x] = new Cell(cellPreset.coords.y, cellPreset.coords.x, position, this, layout != null && layout.Cells != null ? layout.GetCell(cellPreset.coords.x,  + cellPreset.coords.y).state : CellInfo.CellState.Empty);
        }
    }

    public Vector2Int WorldToCell(Vector3 position)
    {
        var rowValue = (position.z - (gameObject.transform.position.z - rowPadding * layout.rows / 2f)) / rowPadding;
        var colValue = (position.x - (gameObject.transform.position.x - columnPadding * layout.columns / 2f)) / columnPadding;

        var row = Mathf.FloorToInt(rowValue);
        var col = Mathf.FloorToInt(colValue);

        row = Mathf.Clamp(row, 0, layout.rows - 1);
        col = Mathf.Clamp(col, 0, layout.columns - 1);
        return new Vector2Int(col, row);
    }

    public Cell GetCell(Vector2Int cellPosition)
    {
        return grid[cellPosition.y, cellPosition.x];
    }

    public Cell GetClosestAvailableCellToPosition(Vector3 position, Vector3 direction)
    {
        Debug.DrawLine(new Vector3(position.x - 5f, 5f, position.z), new Vector3(position.x + 5f, 5f, position.z), Color.magenta);
        Debug.DrawLine(new Vector3(position.x, 5f, position.z - 5f), new Vector3(position.x, 5f, position.z + 5f), Color.magenta);

        Vector2Int cell = WorldToCell(position);

        var startCell = grid[cell.y, cell.x];

        if (!startCell.IsOccupied && startCell.cellInfo.state != CellInfo.CellState.Wall)
        {
            selectedCell = grid[cell.y, cell.x];
            return grid[cell.y, cell.x];
        }

        // continue the search
        var unoccupiedCell = SearchSurroundingCellsForAvailable(startCell, direction);
        selectedCell = unoccupiedCell;

        return unoccupiedCell;
    }

    // NOTE: Could be further optimized by storing the current row and col offset on the wave
    /// <summary>
    /// Searches for the nearest unoccupied cell by percolating outwards from the start cell.
    /// </summary>
    /// <param name="startCell">The start cell.</param>
    /// <returns>The first unoccupied cell found.</returns>
    private Cell SearchSurroundingCellsForAvailable(Cell startCell, Vector3 direction)
    {
        int maxOffset = (layout.rows > layout.columns) ? layout.rows : layout.columns;
        Vector2Int startCoords = startCell.cellInfo.coords;
        for (int offset = 1; offset <= maxOffset; offset++)
        {
            if (direction.y < 0)
            {
                for (int y = -offset; y <= offset; y++)
                {
                    if (direction.x < 0)
                    {
                        for (int x = -offset; x <= offset; x++)
                        {
                            var index = new Vector2Int(startCoords.x + x, startCoords.y + y);
                            if (index.x >= 0 && index.x < layout.columns && index.y >= 0 && index.y < layout.rows
                                && index != startCoords)
                            {
                                var cell = grid[index.y, index.x];
                                if (!cell.IsOccupied && cell.cellInfo.state != CellInfo.CellState.Wall)
                                {
                                    Debug.Log($"Found unoccupied cell: ({cell.cellInfo.coords.x}, {cell.cellInfo.coords.y})");
                                    return cell;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int x = offset; x >= -offset; x--)
                        {
                            var index = new Vector2Int(startCoords.x + x, startCoords.y + y);
                            if (index.x >= 0 && index.x < layout.columns && index.y >= 0 && index.y < layout.rows
                                && index != startCoords)
                            {
                                var cell = grid[index.y, index.x];
                                if (!cell.IsOccupied && cell.cellInfo.state != CellInfo.CellState.Wall)
                                {
                                    Debug.Log($"Found unoccupied cell: ({cell.cellInfo.coords.x}, {cell.cellInfo.coords.y})");
                                    return cell;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int y = offset; y >= -offset; y--)
                {
                    if (direction.x < 0)
                    {
                        for (int x = -offset; x <= offset; x++)
                        {
                            var index = new Vector2Int(startCoords.x + x, startCoords.y + y);
                            if (index.x >= 0 && index.x < layout.columns && index.y >= 0 && index.y < layout.rows
                                && index != startCoords)
                            {
                                var cell = grid[index.y, index.x];
                                if (!cell.IsOccupied && cell.cellInfo.state != CellInfo.CellState.Wall)
                                {
                                    Debug.Log($"Found unoccupied cell: ({cell.cellInfo.coords.x}, {cell.cellInfo.coords.y})");
                                    return cell;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int x = offset; x >= -offset; x--)
                        {
                            var index = new Vector2Int(startCoords.x + x, startCoords.y + y);
                            if (index.x >= 0 && index.x < layout.columns && index.y >= 0 && index.y < layout.rows
                                && index != startCoords)
                            {
                                var cell = grid[index.y, index.x];
                                if (!cell.IsOccupied && cell.cellInfo.state != CellInfo.CellState.Wall)
                                {
                                    Debug.Log($"Found unoccupied cell: ({cell.cellInfo.coords.x}, {cell.cellInfo.coords.y})");
                                    return cell;
                                }
                            }
                        }
                    }
                }
            }
        }

        return null;
    }

    private void DrawCellsGizmos()
    {
        for (int row = 0; row < layout.rows; row++)
        {
            for (int col = 0; col < layout.columns; col++)
            {
                if (selectedCell != null && selectedCell.cellInfo.coords.y == row && selectedCell.cellInfo.coords.x == col)
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

        while (toSearch.Any() && toSearch.Count <= (startNode.cell.Grid.layout.rows * startNode.cell.Grid.layout.columns))
        {
            // The first entry in the list has the current 'best' F value (or H value if they're equal).
            // This is achieved by sorting the list of nodes once the list has been populated.
            var current = toSearch[0]; 
            if (!current.processedOnce)
            {
                current.processedOnce = true;
            }
            processed.Add(current);
            toSearch.Remove(current);

            if (current.cell == targetNode.cell)
            {
                var currentPathTile = current;
                var path = new Stack<Cell>(); // Using a stack ensures we have the list of moves in the right order.
                Debug.Log($"Start cell: ({startNode.cell.cellInfo.coords.x}, {startNode.cell.cellInfo.coords.y})");
                
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
                    
                    Debug.Log($"Current cell: ({currentPathTile.cell.cellInfo.coords.x}, {currentPathTile.cell.cellInfo.coords.y})");
                }

                return path;
            }

            foreach (var neighbor in current.Neighbors.Where(t => t.cell.cellInfo.state != CellInfo.CellState.Wall && !processed.Contains(t)))
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
        return new Stack<Cell>();
    }

    public int GetRows()
    {
        return layout.rows;
    }

    public int GetColumns()
    {
        return layout.columns;
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
                // If the F and H costs match, preference nodes that haven't been processed at least once.
                if (!a.processedOnce && b.processedOnce)
                {
                    return -1;
                }
                else if (a.processedOnce && !b.processedOnce)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
        else
        {
            return 1;
        }
    }
}
