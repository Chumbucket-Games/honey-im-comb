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

    [SerializeField] float baseAttackDelay = 0;
    [SerializeField] float baseAttackDamage = 5;
    float attackRateMultiplier = 1;
    float damageMultiplier = 1;
    
    public bool IsMovable()
    {
        return false;
    }

    private void OnDrawGizmos()
    {
        if (type.canAttack)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, type.attackRadius);
        }
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
        HUDManager.GetInstance().SetSelectedObjectDetails(type.label, (int)health, 0, 0);
        HUDManager.GetInstance().SetActionImages(type.actionSprites);
        HUDManager.GetInstance().ShowSelectedObjectDetails(selectionView);
    }

    public void OnDeselect()
    {
        if (selectionRing != null)
        {
            selectionRing.gameObject.SetActive(false);
        }
        else if (meshRenderer != null)
        {
            meshRenderer.material = defaultMaterial;
        }
        isSelected = false;
        HUDManager.GetInstance().SetActionImages(null);
        HUDManager.GetInstance().HideSelectedObjectDetails(selectionView);
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.HiveManagement.Action1.performed += context =>
            {
                if (isSelected)
                {
                    if (type.label == "Throne Room")
                        GrowBee();
                    else
                        DismantleBuilding();
                }
            };
        }
        playerControls.HiveManagement.Enable();
    }

    private void OnDisable()
    {
        playerControls.HiveManagement.Disable();
    }

    public void GrowBee()
    {
        if (type.label == "Throne Room" && MapController.GetTotalHoney() >= GetComponentInParent<HexGrid>().unitHoneyCost)
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
        if (type.canDismantle)
        {
            Debug.Log($"{type.name} dismantled. All assigned units reverted to worker status.");
            if (hiveBuilding != null)
            {
                hiveBuilding.MaxHealth -= type.extraHiveHealth;
                hiveBuilding.health = Mathf.Max(1, hiveBuilding.health - type.extraHiveHealth);
                hiveBuilding.attackRateMultiplier -= type.extraHiveFirepower;
                hiveBuilding.damageMultiplier -= type.extraHiveFirepower / 2f;
            }

            foreach (var unit in AssignedUnits)
            {
                // Unassign all assigned bees.
                unit.ExchangeUnit(-1);
            }
            GetComponentInParent<HexGrid>().DismantleBuildingCell(GetComponent<HexCell>().Index);
        }
    }

    IEnumerator Attack(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        // if the building can attack, fire projectiles at the closest enemy.
        Enemy closestEnemy = null;
        float closestEnemyDistance = 999;
        foreach (var collider in Physics.OverlapSphere(transform.position, type.attackRadius))
        {
            if (collider.gameObject.GetComponent<Enemy>())
            {
                if (closestEnemy == null)
                {
                    closestEnemy = collider.gameObject.GetComponent<Enemy>();
                    closestEnemyDistance = (collider.transform.position - transform.position).magnitude;
                }
                else if ((collider.transform.position - transform.position).magnitude < closestEnemyDistance)
                {
                    closestEnemy = collider.gameObject.GetComponent<Enemy>();
                    closestEnemyDistance = (collider.transform.position - transform.position).magnitude;
                }
            }
        }

        // If no enemies are in range, stop attacking. Otherwise, attack the closest enemy.
        if (closestEnemy != null)
        {
            // Damage the enemy and delay to the next attack check.
            closestEnemy.TakeDamage(baseAttackDamage * damageMultiplier);
            StartCoroutine(Attack(baseAttackDelay * (1f / attackRateMultiplier)));
        }
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
            hiveBuilding.damageMultiplier += type.extraHiveFirepower / 2f;
        }
    }

    private void Update()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = Mathf.Clamp01(health / MaxHealth);
        }

        if (type.canAttack)
        {
            StartCoroutine(Attack(0));
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
        return GetType();
    }
}
