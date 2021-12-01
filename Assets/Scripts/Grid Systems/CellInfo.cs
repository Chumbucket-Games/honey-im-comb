using UnityEngine;
using System.Collections;

[System.Serializable]
public struct CellInfo
{
    public enum CellState
    {
        Empty = 0,
        Wall = 1,
        GroundWall = 2, // Ground walls will block ground units but not flying units.
    }
    public Vector2Int coords; // Coordinates in grid space
    public CellState state;
}
