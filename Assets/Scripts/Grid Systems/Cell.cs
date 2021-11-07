using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    private int rowIndex;
    private int colIndex;
    private Vector3 position;
    private bool isOccupied;

    public Cell(int rowIndex, int colIndex, Vector3 position)
    {
        this.rowIndex = rowIndex;
        this.colIndex = colIndex;
        this.position = position;

        isOccupied = false;
    }

    public void DrawCell(float rowPadding, float colPadding)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(position, new Vector3(rowPadding, 0f, colPadding));
    }
}
