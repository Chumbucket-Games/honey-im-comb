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
        selectedCell = GetClosestAvailableCellToPosition(new Vector3(31.3f, 0, 30.9f));

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
            selectedCell = GetClosestAvailableCellToPosition(new Vector3(31.3f, 0, 30.9f));
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


    // TODO: Need to think about how we want to implement this
    public Cell GetClosestAvailableCellToPosition(Vector3 position)
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(position.x - 5f, 5f, position.z), new Vector3(position.x + 5f, 5f, position.z));
            Gizmos.DrawLine(new Vector3(position.x, 5f, position.z - 5f), new Vector3(position.x, 5f, position.z + 5f));
        }

        var rowValue = ((gameObject.transform.position.z / 2f) - position.z) / rowPadding;
        var colValue = ((gameObject.transform.position.x / 2f) - position.x) / columnPadding;

        var row = Mathf.FloorToInt(rowValue);
        var col = Mathf.FloorToInt(colValue);

        row = Mathf.Clamp(row, 0, (int)rows);
        col = Mathf.Clamp(col, 0, (int)columns);

        var startCell = grid[row, col];

        if (!startCell.IsOccupied)
        {
            return grid[row, col];
        }

        // continue the search
        SearchSurroundingCellsForAvailable(startCell);

        return null;
    }

    // TODO: need to consider a better way to scan surrounding cells, maybe we should just look out along the column first then row
    public Cell SearchSurroundingCellsForAvailable(Cell startCell)
    {
        // search leftmost cell
        if (startCell.ColIndex - 1 >= 0)
        {
            var cell = grid[startCell.RowIndex, startCell.ColIndex - 1];
            if (!cell.IsOccupied) return cell;
        }

        if (startCell.ColIndex + 1 <= columns)
        {
            var cell = grid[startCell.RowIndex, startCell.ColIndex + 1];
            if (!cell.IsOccupied) return cell;
        }

        if (startCell.RowIndex - 1 >= 0)
        {
            var cell = grid[startCell.RowIndex - 1, startCell.ColIndex];
            if (!cell.IsOccupied) return cell;
        }

        if (startCell.ColIndex + 1 <= rows)
        {
            var cell = grid[startCell.RowIndex + 1, startCell.ColIndex];
            if (!cell.IsOccupied) return cell;
        }

        return null;
    }

    public Cell GetNextAvailableCell()
    {
        for (int row = nextAvailableCellIndex.x; row < rows; row++)
        {
            for (int col = nextAvailableCellIndex.y; col < columns; col++)
            {
                var currentCell = grid[row, col];

                if (!currentCell.IsOccupied)
                {
                    nextAvailableCellIndex = new Vector2Int(row, col);
                    return grid[row, col];
                }
            }
        }

        IsGridFull = true;

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
