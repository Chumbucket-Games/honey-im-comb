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

    public static HexCoordinates ToOffsetCoordinates(int x, int y)
    {
        return new HexCoordinates(x + y * 2, y);
    }

    public static HexCoordinates FromPosition(Vector3 position)
    {
        Vector3 zeroed = position;
        zeroed.z = 0;
        float x = position.x / (HexMetrics.innerRadius * 2f);
        float y = -x;

        float offset = position.z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        if (iX + iY + iZ != 0)
        {
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iY);
    }
}