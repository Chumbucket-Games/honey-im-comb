using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
	public int width = 6;
	public int height = 6;

	public HexCell cellPrefab;

	HexCell[] cells;
	void Awake()
	{
		cells = new HexCell[height * width];

		for (int y = 0, i = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				CreateCell(x, y, i++);
			}
		}
	}

	void CreateCell(int x, int y, int i)
	{
		Vector3 position = new Vector3
		{
			x = (x + y * 0.5f - y / 2) * (HexMetrics.innerRadius * 2f),
			z = 0f,
			y = y * (HexMetrics.outerRadius * 1.5f)
		};

		HexCell cell = cells[i] = Instantiate(cellPrefab);
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, y);
	}
}
