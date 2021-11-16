using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HexGrid : MonoBehaviour, PlayerControls.IHiveManagementActions
{
	public int width = 6;
	public int height = 6;

	public HexCell emptyCellPrefab;
	public HexCell exitPrefab;
	public BuildingType barracks;
	public BuildingType storeroom;
	public BuildingType sentry;
	public BuildingType throne;
	public BuildingType lab;
	public BuildingType wall;

	BuildingType selectedBuilding;

	HexCell[] cells;
	[SerializeField] MapController mapController;
	bool IsBuildMode = false;
	Vector2 cursorPosition;
	PlayerControls playerControls;

	void Awake()
	{
		cells = new HexCell[height * width];

		for (int y = 0, i = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (x == width / 2 && y == 0)
				{
					CreateCell(x, y, i++, exitPrefab);
				}
				else
				{
					CreateCell(x, y, i++, emptyCellPrefab);
				}
			}
		}
		int throneIndex;
		// Place the throne in the center of the hive.
		if ((width * height) % 2 == 0)
		{
			// If the grid size is even, we need this offset to ensure it is placed in the middle and not the edges.
			throneIndex = (width * height / 2) + (width / 2);
		}
		else
		{
			// If the grid size is odd, this will work as-is.
			throneIndex = width * height / 2;
		}
		if (!throne.PlaceBuilding(this, throneIndex))
		{
			throw new System.Exception("Unable to place throne in center of hive.");
		}
	}

	#region Cell manipulation

	/// <summary>
	/// Creates a cell in the Hex grid.
	/// </summary>
	/// <param name="x">X coordinate</param>
	/// <param name="y">Y coordinate</param>
	/// <param name="i">Array index</param>
	/// <param name="newCell">The cell to place</param>
	void CreateCell(int x, int y, int i, HexCell newCell)
	{
		Vector3 position = new Vector3
		{
			x = (x + y * 0.5f - y / 2) * (HexMetrics.innerRadius * 2f),
			z = 0f,
			y = y * (HexMetrics.outerRadius * 1.5f)
		};
		HexCell cell = cells[i] = Instantiate(newCell);
		cell.Index = i;
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, y);
	}

	/// <summary>
	/// Replace the cells with the given indices with a new cell type.
	/// </summary>
	/// <param name="cellsToReplace">The indices of the cells to replace.</param>
	/// <param name="layout">The layout of the cells. This is used for validating building placement.</param>
	/// <param name="newCell">The new cell</param>
	/// <returns>True if all cells were replaced successfully; otherwise false.</returns>
	public bool ReplaceCells(int[] cellsToReplace, BuildingType.LayoutType layout, HexCell newCell)
	{
		for (int i = 0; i < cellsToReplace.Length; i++)
		{
			if (!ValidateCell(cellsToReplace[i]))
			{
				return false;
			}
		}
		if (!ValidateCellNeighbours(cellsToReplace, layout))
		{
			return false;
		}
		for (int i = 0; i < cellsToReplace.Length; i++)
		{
			if (i == 0)
			{
				ReplaceCell(cellsToReplace[i], newCell);
			}
			else
			{
				ReplaceCell(cellsToReplace[i], null);
			}
		}
		return true;
	}

	/// <summary>
	/// Replace the cell at the specified index with a different cell.
	/// </summary>
	/// <param name="index">The index of the cell to replace.</param>
	/// <param name="newCell">The new cell</param>
	void ReplaceCell(int index, HexCell newCell)
	{
		Vector3 position = cells[index].transform.localPosition;
		HexCoordinates coord = cells[index].coordinates;
		Destroy(cells[index].gameObject);
		if (newCell != null)
		{
			HexCell cell = cells[index] = Instantiate(newCell);
			cell.Index = index;
			cell.transform.SetParent(transform, false);
			cell.transform.localPosition = position;
			cell.coordinates = coord;
		}
	}

	#endregion

	#region Cell validation

	bool ValidateCell(int cellIndex)
	{
		if (cellIndex < 0 || cellIndex >= width * height)
		{
			// Cell falls outside of the grid bounds.
			return false;
		}
		if (cells[cellIndex] == null || (cells[cellIndex].GetComponent<Building>() && cells[cellIndex].GetComponent<Building>().type != emptyCellPrefab.GetComponent<Building>().type))
		{
			// Building has been placed in this cell.
			return false;
		}
		return true;
	}

	/// <summary>
	/// Validate the cell neighbours. This will prevent buildings wrapping over edges of the grid.
	/// </summary>
	/// <param name="cellIndex">The indices of the cells to validate</param>
	/// <param name="layout">The layout to validate</param>
	/// <returns>True if all cells have the expected hex grid coordinates; otherwise false</returns>
	bool ValidateCellNeighbours(int[] cellIndex, BuildingType.LayoutType layout)
	{
		List<HexCoordinates> cellPositions = new List<HexCoordinates>();
		foreach (var index in cellIndex)
		{
			cellPositions.Add(cells[index].coordinates);
		}
		switch (layout)
		{
			case BuildingType.LayoutType.Double:
				// Invalid if the right cell is not next to the origin.
				if (cellPositions[1].X - 1 != cellPositions[0].X)
				{
					return false;
				}
				break;
			case BuildingType.LayoutType.Triangle:
				// Invalid if the bottom cells are not next to each other.
				if (cellPositions[1].X + 1 != cellPositions[2].X)
				{
					return false;
				}
				break;
			case BuildingType.LayoutType.Diamond:
				// Invalid if the middle cells are not next to each other.
				if (cellPositions[1].X + 1 != cellPositions[2].X)
				{
					return false;
				}
				break;
			case BuildingType.LayoutType.Hexagon:
				// Invalid if the middle edge cells are not next to the origin.
				if (cellPositions[3].X + 1 != cellPositions[0].X || cellPositions[4].X - 1 != cellPositions[0].X)
				{
					return false;
				}
				break;
		}
		return true;
	}

	#endregion

	/// <summary>
	/// Converts a hex cell coordinate to world space.
	/// </summary>
	/// <param name="x">X coordinate</param>
	/// <param name="y">Y coordinate</param>
	/// <returns>The world-space coordinate of the hex grid coordinate</returns>
	public Vector3 HexCellToWorld(int x, int y)
	{
		Vector3 position = new Vector3
		{
			x = (x + y * 0.5f - y / 2) * (HexMetrics.innerRadius * 2f),
			z = -5f,
			y = y * (HexMetrics.outerRadius * 1.5f)
		};
		return transform.position + position;
	}

	#region Input Handlers

	private void OnEnable()
	{
		if (playerControls == null)
		{
			playerControls = new PlayerControls();
			playerControls.HiveManagement.SetCallbacks(this);
		}
		playerControls.HiveManagement.Enable();
	}

	private void OnDisable()
	{
		playerControls.HiveManagement.Disable();
	}

	public void OnBuildWall(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			SelectBuilding(wall);
		}
	}

	public void OnBuildStoreroom(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			SelectBuilding(storeroom);
		}
	}

	public void OnBuildBarracks(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			SelectBuilding(barracks);
		}
	}

	public void OnBuildLaboratory(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			SelectBuilding(lab);
		}
	}

	public void OnBuildSentry(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			SelectBuilding(sentry);
		}
	}

	public void OnCursor(InputAction.CallbackContext context)
	{
		cursorPosition = context.ReadValue<Vector2>();
	}

	public void OnPlaceBuilding(InputAction.CallbackContext context)
	{
		if (context.performed && selectedBuilding != null && IsBuildMode && mapController.IsHiveMode)
		{
			Ray ray = Camera.main.ScreenPointToRay(cursorPosition);
			if (Physics.Raycast(ray, out var hit))
			{
				if (hit.collider.CompareTag("Building") && hit.transform.gameObject.GetComponent<Building>().type == emptyCellPrefab.GetComponent<Building>().type)
				{
					if (selectedBuilding.PlaceBuilding(this, hit.transform.gameObject.GetComponent<HexCell>().Index))
					{
						Debug.Log($"{selectedBuilding.label} has been built.");
						// Spend resources on the building. In a future commit, the allocated cells will require a worker bee present to construct the building over time.

						// Switch off build mode.
						IsBuildMode = false;
						mapController.SetBuildMode(false);
						selectedBuilding = null;
					}
					else
					{
						Debug.Log("Cannot place building here.");
					}
				}
			}
		}
	}

	void SelectBuilding(BuildingType type)
	{
		IsBuildMode = true;
		mapController.SetBuildMode(true);
		selectedBuilding = type;
		Debug.Log($"Build mode active. {type.label} selected.");
	}

	public void OnCancelBuild(InputAction.CallbackContext context)
	{
		if (context.performed && IsBuildMode)
		{
			IsBuildMode = false;
			mapController.SetBuildMode(false);
			selectedBuilding = null;
			Debug.Log("Build mode cancelled.");
		}
	}

    #endregion
}
