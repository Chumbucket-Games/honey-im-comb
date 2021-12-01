using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public CellInfo cellInfo;
    public SquareGrid Grid { get; private set; }
    public Vector3 Position { get; private set; }
    public bool IsOccupied { get; private set; } = false;
    public bool IsWall { get; private set; }

    public Cell(int rowIndex, int colIndex, Vector3 position, SquareGrid grid)
    {
        cellInfo.coords = new Vector2Int(colIndex, rowIndex);
        cellInfo.state = CellInfo.CellState.Empty;
        Position = position;
        Grid = grid;
    }

    public void DrawCellGizmos(float rowPadding, float colPadding, Color color, float heightOffset)
    {
        if (cellInfo.state == CellInfo.CellState.Wall)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(Position + Vector3.up * heightOffset, new Vector3(colPadding, 0f, rowPadding));
        }
        else if (IsOccupied)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawCube(Position + Vector3.up * heightOffset, new Vector3(colPadding, 0f, rowPadding));
        }
        else
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(Position + Vector3.up * heightOffset, new Vector3(colPadding, 0f, rowPadding));
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
        cellInfo.state = CellInfo.CellState.Wall;
        IsWall = true;
    }
}
