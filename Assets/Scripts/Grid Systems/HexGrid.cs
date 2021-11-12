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
	}

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

	void CreateCell(int x, int y, int i, HexCell newCell)
	{
		Vector3 position = new Vector3
		{
			x = (x + y * 0.5f - y / 2) * (HexMetrics.innerRadius * 2f),
			z = 0f,
			y = y * (HexMetrics.outerRadius * 1.5f)
		};
		Debug.Log(position);
		HexCell cell = cells[i] = Instantiate(newCell);
		cell.index = i;
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, y);
	}

	public void ReplaceCell(int index, HexCell newCell)
	{
		Vector3 position = cells[index].transform.localPosition;
		HexCoordinates coord = cells[index].coordinates;
		Destroy(cells[index].gameObject);
		if (newCell != null)
		{
			HexCell cell = cells[index] = Instantiate(newCell);
			cell.index = index;
			cell.transform.SetParent(transform, false);
			cell.transform.localPosition = position;
			cell.coordinates = coord;
		}
	}

	/*public void ReplaceCells(Vector2Int[] cellsToReplace, HexCell[] newCells)
	{
		for (int i = 0; i < cellsToReplace.Length; i++)
		{
			int x = cellsToReplace[i].x;
			int y = cellsToReplace[i].y;

			Destroy(cells[y * width + x].gameObject);
			CreateCell(x, y, y * width + x, newCells[i]);
		}
	}*/

	public void ReplaceCells(int[] cellsToReplace, HexCell newCell)
	{
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
	}


	/*public Vector2Int WorldToHexCell(Vector3 worldPosition)
	{
		Vector2Int position = new Vector2Int
		{
			x = Mathf.FloorToInt((transform.position.x - (worldPosition.x + worldPosition.y * 0.5f - worldPosition.y / 2)) / (HexMetrics.innerRadius * 2f)),
			y = Mathf.FloorToInt((transform.position.y - worldPosition.y) / (HexMetrics.outerRadius * 1.5f))
		};
		return position;
	}*/

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

	public void OnBuildThrone(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			SelectBuilding(throne);
		}
	}

	public void OnCursor(InputAction.CallbackContext context)
	{
		cursorPosition = context.ReadValue<Vector2>();
	}

	public void OnPlaceBuilding(InputAction.CallbackContext context)
	{
		if (context.performed && selectedBuilding != null && IsBuildMode)
		{
			Ray ray = Camera.main.ScreenPointToRay(cursorPosition);
			if (Physics.Raycast(ray, out var hit))
			{
				if (hit.collider.CompareTag("Building") && hit.transform.gameObject.GetComponent<Building>().type == emptyCellPrefab.GetComponent<Building>().type)
				{
					HexCoordinates coord = hit.transform.gameObject.GetComponent<HexCell>().coordinates;
					Vector3 position = new Vector3
					{
						x = (coord.X + coord.Y * 0.5f - coord.Y / 2) * (HexMetrics.innerRadius * 2f),
						z = 0f,
						y = coord.Y * (HexMetrics.outerRadius * 1.5f)
					};
					selectedBuilding.PlaceBuilding(this, hit.transform.gameObject.GetComponent<HexCell>().index);//new Vector2Int { x = coord.X, y = coord.Y });
					Debug.Log($"{selectedBuilding.label} has been built.");
					IsBuildMode = false;
					mapController.SetBuildMode(false);
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
}
