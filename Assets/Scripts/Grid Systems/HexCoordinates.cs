using UnityEngine;
using UnityEditor;

[System.Serializable]
public struct HexCoordinates
{
    private int x, y;
    public int X
    {
        get
        {
            return x;
        }
    }
    
    public int Y
    {
        get
        {
            return y;
        }
    }

    public int Z
    {
        get
        {
            return -X - Y;
        }
    }
    
    public HexCoordinates (int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static HexCoordinates FromOffsetCoordinates(int x, int y)
    {
        return new HexCoordinates(x - y / 2, y);
    }
}