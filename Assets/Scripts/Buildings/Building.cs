using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Building : MonoBehaviour, ISelectable
{
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] Unit workerPrefab;
    [SerializeField] GameObject selectionRing;
    [SerializeField] Camera selectionView;
    [SerializeField] Image healthBar;
    [SerializeField] Notification underAttackNotification;

    private MeshRenderer meshRenderer;

    float health;
    float MaxHealth;
    public bool IsDead { get; private set; } = false;
    public BuildingType type;
    public List<Unit> AssignedUnits { get; private set; }
    PlayerControls playerControls;
    
    bool isSelected;

    [SerializeField] Building hiveBuilding;

    [SerializeField] float baseAttackRate = 0;
    int attackRateMultiplier = 1;
    
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
        if (selectionRing != null)
        {
            selectionRing.gameObject.SetActive(true);
        }
        else
        {
            meshRenderer.material = selectedMaterial;
        }

        Debug.Log($"{type.label} selected.");
        isSelected = true;
        if (selectionView != null)
        {
            selectionView.gameObject.SetActive(true);
        }
        HUDManager.GetInstance().SetSelectedObjectDetails(type.label, (int)health, 0, 0);
        HUDManager.GetInstance().SetActionImages(type.actionSprites);
    }

    public void OnDeselect()
    {
        meshRenderer.material = defaultMaterial;
        if (selectionRing != null)
        {
            selectionRing.gameObject.SetActive(false);
        }
        isSelected = false;
        if (selectionView != null)
        {
            selectionView.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.HiveManagement.Action1.performed += context => GrowBee();
            playerControls.HiveManagement.Action2.performed += context => DismantleBuilding();
        }
        playerControls.HiveManagement.Enable();
    }

    private void OnDisable()
    {
        playerControls.HiveManagement.Disable();
    }

    public void GrowBee()
    {
        if (isSelected && type.name == "Throne" && MapController.GetTotalHoney() >= GetComponentInParent<HexGrid>().unitHoneyCost)
        {
            // Create a bee unit in front of a random unoccupied cell.
            HexCell cell;
            do
            {
                cell = GetComponentInParent<HexGrid>().SelectRandomCell();
            } while (cell.IsOccupied);

            Unit unit = Instantiate(workerPrefab, cell.transform.position + new Vector3(0, 0, GameConstants.HiveUnitOffset), Quaternion.identity);
            unit.SetTargetObject(cell.gameObject);
            HexGrid.IncreaseTotalUnits(1);
            Debug.Log("A new worker is born!");
        }
    }

    public void DismantleBuilding()
    {
        if (isSelected && type.canDismantle)
        {
            Debug.Log($"{type.name} dismantled. All assigned units reverted to worker status.");
            foreach (var unit in AssignedUnits)
            {
                // Unassign all assigned bees.
                unit.ExchangeUnit(-1);
            }
            GetComponentInParent<HexGrid>().DismantleBuildingCell(GetComponent<HexCell>().Index);
        }
    }

    public void Attack()
    {
        // if the building can attack, fire projectiles at the closest enemy.
    }

    // Use this for initialization
    void Start()
    {
        MaxHealth = type.maxHealth;
        health = type.maxHealth;
        meshRenderer = GetComponent<MeshRenderer>();
        AssignedUnits = new List<Unit>();

        if (hiveBuilding != null)
        {
            hiveBuilding.MaxHealth += type.extraHiveHealth;
            hiveBuilding.health += type.extraHiveHealth;
            hiveBuilding.attackRateMultiplier += type.extraHiveFirepower;
        }
        
    }

    private void Update()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = Mathf.Clamp01(health / MaxHealth);
        }
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

        if (isSelected)
        {
            HUDManager.GetInstance().SetSelectedObjectHealth((int)health);
        }

        if (underAttackNotification != null)
        {
            HUDManager.GetInstance().CreateNotification(underAttackNotification);
        }

        if (health <= 0)
        {
            if (isSelected)
            {
                OnDeselect();
            }
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

    public System.Type GetObjectType()
    {
        return this.GetType();
    }
}
