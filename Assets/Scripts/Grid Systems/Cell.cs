using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public SquareGrid grid { get; private set; }
    public int RowIndex { get; private set; }
    public int ColIndex { get; private set; }
    public Vector3 Position { get; private set; }
    public bool IsOccupied { get; private set; }
    public bool IsWall { get; private set; }

    public Cell(int rowIndex, int colIndex, Vector3 position, SquareGrid grid)
    {
        RowIndex = rowIndex;
        ColIndex = colIndex;
        Position = position;
        this.grid = grid;

        IsOccupied = false;
    }

    public void DrawCellGizmos(float rowPadding, float colPadding, Color color)
    {
        if (IsWall)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(Position + Vector3.up * 5, new Vector3(colPadding, 0f, rowPadding));
        }
        else
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(Position + Vector3.up * 5, new Vector3(colPadding, 0f, rowPadding));
        }
    }

    public void OccupyCell()
    {
        IsOccupied = true;
    }

    public void EmptyCell()
    {
        IsOccupied = false;
    }

    public void SetWall()
    {
        IsWall = true;
    }
}
