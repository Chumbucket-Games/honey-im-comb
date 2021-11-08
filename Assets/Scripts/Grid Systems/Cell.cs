using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    private int rowIndex;
    private int colIndex;
    public Vector3 Position { get; private set; }
    public bool IsOccupied { get; private set; }

    public Cell(int rowIndex, int colIndex, Vector3 position)
    {
        this.rowIndex = rowIndex;
        this.colIndex = colIndex;
        this.Position = position;

        IsOccupied = false;
    }

    public void DrawCellGizmos(float rowPadding, float colPadding)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Position, new Vector3(rowPadding, 0f, colPadding));
    }

    public void OccupyCell()
    {
        IsOccupied = true;
    }

    public void ClearCell()
    {
        IsOccupied = false;
    }
}
