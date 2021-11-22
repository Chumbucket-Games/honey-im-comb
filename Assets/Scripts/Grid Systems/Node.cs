using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Node
{
    public Node Connection { get; private set; }
    private List<Node> _neighbors;
    public List<Node> Neighbors
    {
        get
        {
            if (_neighbors == null)
            {
                _neighbors = new List<Node>();

                SquareGrid grid = cell.grid;
                Cell neighborCell;
                Node node;

                if (cell.ColIndex > 0)
                {
                    if (cell.RowIndex > 0)
                    {
                        // Bottom left
                        neighborCell = grid.GetCell(cell.ColIndex - 1, cell.RowIndex - 1);
                        node = CreateNode(neighborCell);
                        //node.G = G + GetDistance(neighborCell);
                        _neighbors.Add(node);
                    }
                    if (cell.RowIndex < grid.GetRows() - 1)
                    {
                        // Top left
                        neighborCell = grid.GetCell(cell.ColIndex - 1, cell.RowIndex + 1);
                        node = CreateNode(neighborCell);
                        //node.G = G + GetDistance(neighborCell);
                        _neighbors.Add(node);

                    }
                    // Middle left.
                    neighborCell = grid.GetCell(cell.ColIndex - 1, cell.RowIndex);
                    node = CreateNode(neighborCell);
                    //node.G = G + GetDistance(neighborCell);
                    _neighbors.Add(node);
                }
                if (cell.ColIndex < grid.GetColumns() - 1)
                {
                    if (cell.RowIndex > 0)
                    {
                        // Bottom right
                        neighborCell = grid.GetCell(cell.ColIndex + 1, cell.RowIndex - 1);
                        node = CreateNode(neighborCell);
                        //node.G = G + GetDistance(neighborCell);
                        _neighbors.Add(node);
                    }
                    if (cell.RowIndex < grid.GetRows() - 1)
                    {
                        // Top right
                        neighborCell = grid.GetCell(cell.ColIndex + 1, cell.RowIndex + 1);
                        node = CreateNode(neighborCell);
                        //node.G = G + GetDistance(neighborCell);
                        _neighbors.Add(node);

                    }
                    // Middle right.
                    neighborCell = grid.GetCell(cell.ColIndex + 1, cell.RowIndex);
                    node = CreateNode(neighborCell);
                    //node.G = G + GetDistance(neighborCell);
                    _neighbors.Add(node);
                }
                if (cell.RowIndex > 0)
                {
                    // Bottom middle
                    neighborCell = grid.GetCell(cell.ColIndex, cell.RowIndex - 1);
                    node = CreateNode(neighborCell);
                    //node.G = G + GetDistance(neighborCell);
                    _neighbors.Add(node);
                }
                if (cell.RowIndex < grid.GetRows() - 1)
                {
                    // Top middle
                    neighborCell = grid.GetCell(cell.ColIndex, cell.RowIndex + 1);
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
        var dist = new Vector2Int(Mathf.Abs(cell.ColIndex - neighbor.cell.ColIndex), Mathf.Abs(cell.RowIndex - neighbor.cell.RowIndex));

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
