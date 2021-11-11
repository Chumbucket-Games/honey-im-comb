using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public int RowIndex { get; private set; }
    public int ColIndex { get; private set; }
    public Vector3 Position { get; private set; }
    public bool IsOccupied { get; private set; }

    public Cell(int rowIndex, int colIndex, Vector3 position)
    {
        this.RowIndex = rowIndex;
        this.ColIndex = colIndex;
        this.Position = position;

        IsOccupied = false;
    }

    public void DrawCellGizmos(float rowPadding, float colPadding, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(Position, new Vector3(colPadding, 0f, rowPadding));
    }

    public void MarkCellAsOccupied()
    {
        IsOccupied = true;
    }

    public void MarkCellAsUnoccupied()
    {
        IsOccupied = false;
    }
}
