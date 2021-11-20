using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Building : MonoBehaviour, ISelectable
{
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] HexCell emptyCellPrefab;
    [SerializeField] Unit workerPrefab;

    private MeshRenderer meshRenderer;

    float health;
    public bool IsDead { get; private set; } = false;
    public BuildingType type;
    public List<Unit> AssignedUnits { get; private set; }
    PlayerControls playerControls;
    
    bool isSelected;
    
    public bool IsMovable()
    {
        return false;
    }

    public void MoveToPosition(Vector3 position, RaycastHit info, bool IsHiveMode)
    {
        throw new System.InvalidOperationException();
    }

    public void OnSelect()
    {
        meshRenderer.material = selectedMaterial;

        Debug.Log($"{type.label} selected.");
        isSelected = true;
    }

    public void OnDeselect()
    {
        meshRenderer.material = defaultMaterial;
        isSelected = false;
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.HiveManagement.Action1.performed += OnAction1;
            playerControls.HiveManagement.Action6.performed += DismantleBuilding;
        }
        playerControls.HiveManagement.Enable();
    }

    private void OnDisable()
    {
        playerControls.HiveManagement.Disable();
    }

    public void OnAction1(InputAction.CallbackContext context)
    {
        if (isSelected && type.name == "Throne" && MapController.GetTotalHoney() > GetComponentInParent<HexGrid>().unitHoneyCost)
        {
            // Create a bee unit in front of a random unoccupied cell.
            HexCell cell;
            do
            {
                cell = GetComponentInParent<HexGrid>().SelectRandomCell();
            } while (cell.IsOccupied);
            
            Unit unit = Instantiate(workerPrefab, cell.transform.position + new Vector3(0, 0, -3.2f), Quaternion.identity);
            unit.SetTargetObject(cell.gameObject);
            HexGrid.IncreaseTotalUnits(1);
            Debug.Log("A new worker is born!");
        }
    }

    public void DismantleBuilding(InputAction.CallbackContext context)
    {
        Debug.Log($"{type.name} dismantled. All assigned units reverted to worker status.");
        if (isSelected && type.canDismantle)
        {
            foreach (var unit in AssignedUnits)
            {
                // Unassign all assigned bees.
                unit.ExchangeUnit(-1);
            }
        }
        GetComponentInParent<HexGrid>().DismantleBuildingCell(GetComponent<HexCell>().Index);
    }

    // Use this for initialization
    void Start()
    {
        health = type.maxHealth;
        meshRenderer = GetComponent<MeshRenderer>();
        AssignedUnits = new List<Unit>();
    }

    public void OnDestroyed()
    {
        type.OnDestroyed(/*this*/);
        IsDead = true;
        Destroy(gameObject);
    }

    public void TakeDamage(int dmg)
    {
        health = Mathf.Max(0, health - dmg);

        if (health <= 0)
        {
            OnDestroyed();
        }
    }

    public void AttachUnit(Unit unit)
    {
        AssignedUnits.Add(unit);
    }

    public void DetachUnit(Unit unit)
    {
        AssignedUnits.Remove(unit);
    }
}
