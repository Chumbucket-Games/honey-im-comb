using UnityEngine;
using UnityEditor;


[CreateAssetMenu(menuName = "Building Type")]
public class BuildingType : ColonyObject
{
    public enum LayoutType
    {
        Single = 1,
        Double,
        Triangle,
        Diamond,
        Hexagon
    }
    public LayoutType gridLayout;
    public ResourceStack pebbles;
    public ResourceStack honey;
    public Unit associatedUnitPrefab = null; // The unit associated with this building.
    public Unit workerPrefab;
    public int totalUnits = 1; // Default to 1 per building, but this can be increased on a per-building basis.
    public HexCell cell; // The cell prefab that represents this building.
    public bool winWhenDestroyed = false;
    public bool loseWhenDestroyed = false;
    public bool canDismantle = true;
    public bool canOccupy = true;
    public bool canAttack = false;
    public int extraHiveHealth = 0;
    public int extraHiveFirepower = 0;
    public int extraResourceStorage = 0;

    
    public bool PlaceBuilding(HexGrid grid, int cellIndex)
    {
        if (MapController.GetTotalPebbles() < pebbles.quantity || MapController.GetTotalHoney() < honey.quantity)
        {
            return false;
        }
        //Vector2Int[] cellPositions;
        int[] cellPositions;
        switch (gridLayout)
        {
            // On even rows for any structure with 3+ cells occupied, the left cell is offset from cellPosition - width by -1.
            // On odd rows, the right cell is offset from cellPosition - width by +1. This is because the even and odd rows are offset from each other in the hex grid.
            case LayoutType.Double:
                // Cursor position is left edge of 1x2 rectangle
                cellPositions = new int[2] {
                    cellIndex, // Left
                    cellIndex + 1 // Right
                };
                break;
            case LayoutType.Triangle:
                // Cursor position is top of triangle.
                if ((cellIndex / grid.height) % 2 == 0)
                {
                    cellPositions = new int[3] {
                        cellIndex, // Top
                        cellIndex - grid.width - 1, // Bottom Left
                        cellIndex - grid.width // Bottom Right
                    };
                }
                else
                {
                    cellPositions = new int[3] {
                        cellIndex, // Top
                        cellIndex - grid.width, // Bottom Left
                        cellIndex - grid.width + 1 // Bottom Right
                    };
                }
                break;
            case LayoutType.Diamond:
                // Cursor position is top of diamond.
                if ((cellIndex / grid.height) % 2 == 0)
                {
                    cellPositions = new int[4] {
                        cellIndex, // Top
                        cellIndex - grid.width - 1, // Mid Left
                        cellIndex - grid.width, // Mid Right
                        cellIndex - (grid.width * 2) // Bottom
                    };
                }
                else
                {
                    cellPositions = new int[4] {
                        cellIndex, // Top
                        cellIndex - grid.width, // Mid Left
                        cellIndex - grid.width + 1, // Mid Right
                        cellIndex - (grid.width * 2) // Bottom
                    };
                }
                
                break;
            case LayoutType.Hexagon:
                // Cursor position is center of a 7-cell hexagon.
                if ((cellIndex / grid.height) % 2 == 0)
                {
                    cellPositions = new int[7] {
                        cellIndex, // Middle
                        cellIndex + grid.width - 1, // Top Left
                        cellIndex + grid.width, // Top Right
                        cellIndex - 1, // Mid Left
                        cellIndex + 1, // Mid Right
                        cellIndex - grid.width - 1, // Bottom Left
                        cellIndex - grid.width // Bottom Right
                    };
                }
                else
                {
                    cellPositions = new int[7] {
                        cellIndex, // Middle
                        cellIndex + grid.width, // Top Left
                        cellIndex + grid.width + 1, // Top Right
                        cellIndex - 1, // Mid Left
                        cellIndex + 1, // Mid Right
                        cellIndex - grid.width, // Bottom Left
                        cellIndex - grid.width + 1 // Bottom Right
                    };
                }
                break;
            default:
                // Place at the cursor position.
                cellPositions = new int[1] { cellIndex };
                break;
        }
        return grid.ReplaceCells(cellPositions, gridLayout, cell);
    }

    public static void SwitchUnit(Unit oldUnit)
    {
        if (oldUnit.type.buildingType != oldUnit.AssociatedBuilding.type)
        {
            Debug.Log("Switching to new role!");
            Unit newUnit = Instantiate(oldUnit.AssociatedBuilding.type.associatedUnitPrefab, oldUnit.transform.position, oldUnit.transform.rotation);
            oldUnit.AssociatedBuilding.AttachUnit(newUnit);
            newUnit.AttachBuilding(oldUnit.AssociatedBuilding);
        }
        else
        {
            Debug.Log("Reverting to worker role");
            Instantiate(oldUnit.AssociatedBuilding.type.workerPrefab, oldUnit.transform.position, oldUnit.transform.rotation);
            oldUnit.AssociatedBuilding.DetachUnit(oldUnit);
            oldUnit.DetachCurrentBuilding();
        }
        
        Destroy(oldUnit.gameObject);
    }

    public void OnDestroyed(/*Building instance*/)
    {
        if (winWhenDestroyed)
        {
            // Win the game.
            MapController.WinGame();
        }
        else if (loseWhenDestroyed)
        {
            // Lose the game.
            MapController.LoseGame();
        }
    }
}