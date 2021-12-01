using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Node
{
    public Node Connection { get; private set; }
    private List<Node> _neighbors;
    public bool processedOnce = false;
    public List<Node> Neighbors
    {
        get
        {
            if (_neighbors == null)
            {
                _neighbors = new List<Node>();

                SquareGrid grid = cell.Grid;
                Cell neighborCell;
                Node node;

                if (cell.cellInfo.coords.x > 0)
                {
                    if (cell.cellInfo.coords.y > 0)
                    {
                        // Bottom left
                        neighborCell = grid.GetCell(cell.cellInfo.coords.x - 1, cell.cellInfo.coords.y - 1);
                        node = CreateNode(neighborCell);
                        //node.G = G + GetDistance(neighborCell);
                        _neighbors.Add(node);
                    }
                    if (cell.cellInfo.coords.y < grid.GetRows() - 1)
                    {
                        // Top left
                        neighborCell = grid.GetCell(cell.cellInfo.coords.x - 1, cell.cellInfo.coords.y + 1);
                        node = CreateNode(neighborCell);
                        //node.G = G + GetDistance(neighborCell);
                        _neighbors.Add(node);

                    }
                    // Middle left.
                    neighborCell = grid.GetCell(cell.cellInfo.coords.x - 1, cell.cellInfo.coords.y);
                    node = CreateNode(neighborCell);
                    //node.G = G + GetDistance(neighborCell);
                    _neighbors.Add(node);
                }
                if (cell.cellInfo.coords.x < grid.GetColumns() - 1)
                {
                    if (cell.cellInfo.coords.y > 0)
                    {
                        // Bottom right
                        neighborCell = grid.GetCell(cell.cellInfo.coords.x + 1, cell.cellInfo.coords.y - 1);
                        node = CreateNode(neighborCell);
                        //node.G = G + GetDistance(neighborCell);
                        _neighbors.Add(node);
                    }
                    if (cell.cellInfo.coords.y < grid.GetRows() - 1)
                    {
                        // Top right
                        neighborCell = grid.GetCell(cell.cellInfo.coords.x + 1, cell.cellInfo.coords.y + 1);
                        node = CreateNode(neighborCell);
                        //node.G = G + GetDistance(neighborCell);
                        _neighbors.Add(node);

                    }
                    // Middle right.
                    neighborCell = grid.GetCell(cell.cellInfo.coords.x + 1, cell.cellInfo.coords.y);
                    node = CreateNode(neighborCell);
                    //node.G = G + GetDistance(neighborCell);
                    _neighbors.Add(node);
                }
                if (cell.cellInfo.coords.y > 0)
                {
                    // Bottom middle
                    neighborCell = grid.GetCell(cell.cellInfo.coords.x, cell.cellInfo.coords.y - 1);
                    node = CreateNode(neighborCell);
                    //node.G = G + GetDistance(neighborCell);
                    _neighbors.Add(node);
                }
                if (cell.cellInfo.coords.y < grid.GetRows() - 1)
                {
                    // Top middle
                    neighborCell = grid.GetCell(cell.cellInfo.coords.x, cell.cellInfo.coords.y + 1);
                    node = CreateNode(neighborCell);
                    //node.G = G + GetDistance(neighborCell);
                    _neighbors.Add(node);
                }
            }

            return _neighbors;
        }
    }
    public Cell cell;
    public float G { get; private set; }
    public float H { get; private set; }
    public float F => G + H;

    public void SetConnection(Node node) => Connection = node;

    public void SetG(float g) => G = g;
    public void SetH(float h) => H = h;

    public Node(Cell cell)
    {
        this.cell = cell;
    }

    public float GetDistance(Node neighbor)
    {
        var dist = new Vector2Int(Mathf.Abs(cell.cellInfo.coords.x - neighbor.cell.cellInfo.coords.x), Mathf.Abs(cell.cellInfo.coords.y - neighbor.cell.cellInfo.coords.y));

        var lowest = Mathf.Min(dist.x, dist.y);
        var highest = Mathf.Max(dist.x, dist.y);
        var horizontalMovesRequired = highest - lowest;

        return lowest * 14 + horizontalMovesRequired * 10;
    }

    Node CreateNode(Cell cell)
    {
        Node newNode = new Node(cell);
        return newNode;
    }
}
