using UnityEngine;
using System.Collections;

public class HexCell : MonoBehaviour
{
    [HideInInspector] public HexCoordinates coordinates;
    public int Index { get; set; }
    public bool IsOccupied { get; set; } = false;
}
