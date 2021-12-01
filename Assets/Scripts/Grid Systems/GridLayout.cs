using UnityEngine;

[CreateAssetMenu(menuName = "Game grid layout")]
public class GridLayout : ScriptableObject
{
    public int rows = 50;
    public int columns = 50;
    public CellInfo[,] cells;

    private void Awake()
    {
        if (cells == null)
        {
            cells = new CellInfo[rows, columns];
        }
    }

    public void SetCellState(Vector2Int cellCoords)
    {
        int newState = (int)cells[cellCoords.y, cellCoords.x].state + 1;
        if (newState > 2)
        {
            newState = 0;
        }
        cells[cellCoords.y, cellCoords.x].state = (CellInfo.CellState)newState;
    }
}