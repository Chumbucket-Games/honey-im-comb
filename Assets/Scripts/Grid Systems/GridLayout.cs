using UnityEngine;

[CreateAssetMenu(menuName = "Game grid layout")]
public class GridLayout : ScriptableObject
{
    public int rows = 50;
    public int columns = 50;
    public CellInfo[] cells;
    public CellInfo[] Cells
    {
        get
        {
            return cells;
        }
    }

    public void InitCells()
    {
        cells = new CellInfo[rows * columns];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                cells[columns * y + x].coords = new Vector2Int(x, y);
            }
        }
    }

    public CellInfo.CellState SetCellState(Vector2Int cellCoords)
    {
        int newState = (int)cells[columns * cellCoords.y + cellCoords.x].state + 1;
        if (newState > 2)
        {
            newState = 0;
        }
        cells[columns * cellCoords.y + cellCoords.x].state = (CellInfo.CellState)newState;
        return (CellInfo.CellState)newState;
    }

    public CellInfo GetCell(int x, int y)
    {
        return Cells[columns * y + x];
    }
}