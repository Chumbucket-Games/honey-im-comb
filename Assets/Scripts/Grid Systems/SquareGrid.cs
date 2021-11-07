using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareGrid : MonoBehaviour
{

    [SerializeField] private uint rows = 5;
    [SerializeField] private float rowPadding = 1f;

    [SerializeField] private uint columns = 5;
    [SerializeField] private float columnPadding = 1f;

    private Cell[,] grid;

    // Start is called before the first frame update
    void Start()
    {
        grid = new Cell[rows, columns];
        InitGridCells();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        grid = null;
        grid = new Cell[rows, columns];
        InitGridCells();
        DrawCells();
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

    private void DrawCells()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                grid[row, col].DrawCell(rowPadding, columnPadding);
            }
        }
    }
}
