using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private Cell[,] grid;
    private Cell selectedCell = null;

    private Vector2Int nextAvailableCellIndex = Vector2Int.zero;

    public bool IsGridFull { get; private set; }
    public uint TotalCells { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        grid = new Cell[rows, columns];
        InitGridCells();

        IsGridFull = false;
        TotalCells = rows * columns;
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
                grid[row, col] = new Cell(row, col, position);
            }
        }
    }

    public Cell GetClosestAvailableCellToPosition(Vector3 position, int maxRowSearchDistance, int maxColumnSearchDistance)
    {
        Debug.DrawLine(new Vector3(position.x - 5f, 5f, position.z), new Vector3(position.x + 5f, 5f, position.z), Color.magenta);
        Debug.DrawLine(new Vector3(position.x, 5f, position.z - 5f), new Vector3(position.x, 5f, position.z + 5f), Color.magenta);
        
        var rowValue = (position.z - (gameObject.transform.position.z - rowPadding * rows / 2f)) / rowPadding;
        var colValue = (position.x - (gameObject.transform.position.x - columnPadding * columns / 2f)) / columnPadding;

        var row = Mathf.FloorToInt(rowValue);
        var col = Mathf.FloorToInt(colValue);

        row = Mathf.Clamp(row, 0, (int)rows);
        col = Mathf.Clamp(col, 0, (int)columns);

        var startCell = grid[row, col];

        if (!startCell.IsOccupied)
        {
            selectedCell = grid[row, col];
            return grid[row, col];
        }

        // continue the search
        var unoccupiedCell = SearchSurroundingCellsForAvailable(startCell, maxRowSearchDistance, maxColumnSearchDistance);
        selectedCell = unoccupiedCell;

        Debug.Log(unoccupiedCell);

        return unoccupiedCell;
    }

    // NOTE: Could be further optimized by storing the current row and col offset on the wave
    private Cell SearchSurroundingCellsForAvailable(Cell startCell, int maxRowSearchDistance, int maxColumnSearchDistance)
    {
        for (int rowOffset = 0; rowOffset < maxRowSearchDistance; rowOffset++)
        {
            // search columns first
            for (int colOffset = 1; colOffset < maxColumnSearchDistance; colOffset++)
            {
                // left first
                if (startCell.ColIndex - colOffset >= 0)
                {
                    var cell = grid[startCell.RowIndex, startCell.ColIndex - colOffset];
                    if (!cell.IsOccupied) return cell;
                }

                // then right
                if (startCell.ColIndex + colOffset < columns)
                {
                    var cell = grid[startCell.RowIndex, startCell.ColIndex + colOffset];
                    if (!cell.IsOccupied) return cell;
                }
            }

            // then down
            if (startCell.RowIndex - rowOffset >= 0)
            {
                var cell = grid[startCell.RowIndex - rowOffset, startCell.ColIndex];
                if (!cell.IsOccupied) return cell;
            }

            // then up
            if (startCell.RowIndex + rowOffset < rows)
            {
                var cell = grid[startCell.RowIndex + rowOffset, startCell.ColIndex];
                if (!cell.IsOccupied) return cell;
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
                    grid[row, col].DrawCellGizmos(rowPadding - .5f, columnPadding - .5f, Color.magenta);
                }
                else
                {
                    grid[row, col].DrawCellGizmos(rowPadding, columnPadding, gridColor);
                }
            }
        }
    }
}
