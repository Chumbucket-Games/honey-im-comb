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

    private Cell[,] grid;

    private Vector2Int nextAvailableCellIndex = Vector2Int.zero;
    public bool IsGridFull { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        grid = new Cell[rows, columns];
        InitGridCells();
        IsGridFull = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
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
                grid[row, col].DrawCellGizmos(rowPadding, columnPadding);
            }
        }
    }
}
