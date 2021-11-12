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
    public int pebbleCost;
    public int honeyCost;
    public HexCell cell;

    public void PlaceBuilding(HexGrid grid, int cellIndex/*Vector2Int gridPosition*/)
    {
        //Vector2Int[] cellPositions;
        int[] cellPositions;
        switch (gridLayout)
        {
            case LayoutType.Single:
                // Place at the cursor position.
                //cellPositions = new Vector2Int[1] { gridPosition };
                cellPositions = new int[1] { cellIndex };
                grid.ReplaceCells(cellPositions, cell);
                break;
            case LayoutType.Double:
                // Cursor position is top left of 1x2 rectangle
                cellPositions = new int[2] {
                    cellIndex, // Top
                    cellIndex - grid.width // Bottom
                };
                 /*cellPositions = new Vector2Int[2] {
                     gridPosition, // Top
                     new Vector2Int { x = gridPosition.x, y = gridPosition.y - 1 } // Bottom
                 };*/
                 grid.ReplaceCells(cellPositions, cell);
                break;
            case LayoutType.Triangle:
                // Cursor position is top of triangle.
                //Vector2Int bottomLeft = 
                /*cellPositions = new Vector2Int[3] {
                    gridPosition, // Top
                    new Vector2Int { x = gridPosition.x - 1, y = gridPosition.y - 1 }, // Bottom Left
                    new Vector2Int { x = gridPosition.x, y = gridPosition.y - 1 }, }; // Bottom Right*/
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
                
                grid.ReplaceCells(cellPositions, cell);
                break;
            case LayoutType.Diamond:
                // Cursor position is top of diamond.
                /*cellPositions = new Vector2Int[4] {
                    gridPosition, // Top
                    new Vector2Int { x = gridPosition.x, y = gridPosition.y - 1 }, // Mid left
                    new Vector2Int { x = gridPosition.x + 1, y = gridPosition.y - 1 }, // Mid right
                    new Vector2Int { x = gridPosition.x, y = gridPosition.y - 2 }, // Bottom
                };*/
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
                grid.ReplaceCells(cellPositions, cell);
                break;
            case LayoutType.Hexagon:
                // Cursor position is center of a 7-cell hexagon.
                /*cellPositions = new Vector2Int[7] {
                    gridPosition, // Middle
                    new Vector2Int { x = gridPosition.x - 1, y = gridPosition.y + 1 }, // Top left
                    new Vector2Int { x = gridPosition.x, y = gridPosition.y + 1 }, // Top right
                    new Vector2Int { x = gridPosition.x - 1, y = gridPosition.y }, // Mid left
                    new Vector2Int { x = gridPosition.x + 1, y = gridPosition.y }, // Mid right
                    new Vector2Int { x = gridPosition.x - 1, y = gridPosition.y - 1 }, // Bottom Left
                    new Vector2Int { x = gridPosition.x, y = gridPosition.y - 1 } // Bottom Right
                };*/
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
                
                grid.ReplaceCells(cellPositions, cell);
                break;
        }
    }
}